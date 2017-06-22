using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements a custom MsBuild task that performs namespace dependency analysis  and reports disallowed dependencies.
    /// </summary>
    public class NsDepCopTask : Task
    {
        public static readonly IssueDescriptor<string> TaskStartedIssue =
            new IssueDescriptor<string>("NSDEPCOPSTART", IssueKind.Info, null, i => $"Analysing project in folder: {i}");

        public static readonly IssueDescriptor<TimeSpan> TaskFinishedIssue =
            new IssueDescriptor<TimeSpan>("NSDEPCOPFINISH", IssueKind.Info, null, i => $"Analysis took: {i:mm\\:ss\\.fff}");

        public static readonly IssueDescriptor<Exception> TaskExceptionIssue =
            new IssueDescriptor<Exception>("NSDEPCOPEX", IssueKind.Error, null, i => $"Exception during NsDepCopTask execution: {i.ToString()}");

        /// <summary>
        /// MsBuild task item list that contains the name and full path 
        /// of the assemblies referenced in the current project.
        /// </summary>
        [Required]
        public ITaskItem[] ReferencePath { get; set; }

        /// <summary>
        /// MsBuild task item list that contains the name and relative path
        /// of the source files in the current project.
        /// The paths are relative to the BaseDirectory.
        /// </summary>
        [Required]
        public ITaskItem[] Compile { get; set; }

        /// <summary>
        /// MsBuild task item that contains the full path of the directory of the project file.
        /// </summary>
        [Required]
        public ITaskItem BaseDirectory { get; set; }

        /// <summary>
        /// Not used any more.
        /// </summary>
        public ITaskItem Parser { get; set; }

        /// <summary>
        /// Specifies the info log events' message importance level. Optional. Default: Normal.
        /// </summary>
        public ITaskItem InfoImportance { get; set; }

        private readonly ILogger _logger;
        private readonly AssemblyBindingRedirector _assemblyBindingRedirector;

        public NsDepCopTask()
        {
            _logger = new MsBuildLoggerGateway(BuildEngine);

            // Must handle assembly binding redirect because MsBuild does not provide it.
            // See: https://github.com/Microsoft/msbuild/issues/1309
            _assemblyBindingRedirector = new AssemblyBindingRedirector(_logger.LogTraceMessage);
        }

        /// <summary>
        /// This ctor is for unit testing. 
        /// </summary>
        /// <param name="logger">A logger object.</param>
        public NsDepCopTask(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IEnumerable<string> SourceFilePaths => Compile.ToList().Select(i => i.ItemSpec);
        private IEnumerable<string> ReferencedAssemblyPaths => ReferencePath.ToList().Select(i => i.ItemSpec);

        /// <summary>
        /// Executes the custom MsBuild task. Called by the MsBuild tool.
        /// </summary>
        /// <returns>
        /// True if there was no error and no exception.
        /// </returns>
        public override bool Execute()
        {
            try
            {
                _logger.LogTraceMessage(GetInputParameterDiagnosticMessages());

                var defaultInfoImportance = ParseNullable<Importance>(InfoImportance.GetValue());
                _logger.InfoImportance = defaultInfoImportance?.ToMessageImportance() ?? MessageImportance.Normal;

                var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(_logger.LogTraceMessage);
                var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(typeDependencyEnumerator, _logger.LogTraceMessage)
                    .SetDefaultInfoImportance(defaultInfoImportance);

                var configFolderPath = BaseDirectory.ItemSpec;
                using (var dependencyAnalyzer = dependencyAnalyzerFactory.CreateFromMultiLevelXmlConfigFile(configFolderPath))
                {
                    var runWasSuccessful = true;

                    switch (dependencyAnalyzer.ConfigState)
                    {
                        case AnalyzerConfigState.NoConfig:
                            _logger.LogIssue(IssueDefinitions.NoConfigFileIssue);
                            break;

                        case AnalyzerConfigState.Disabled:
                            _logger.LogIssue(IssueDefinitions.ConfigDisabledIssue);
                            break;

                        case AnalyzerConfigState.ConfigError:
                            _logger.LogIssue(IssueDefinitions.ConfigExceptionIssue, dependencyAnalyzer.ConfigException);
                            runWasSuccessful = false;
                            break;

                        case AnalyzerConfigState.Enabled:
                            runWasSuccessful = ExecuteAnalysis(dependencyAnalyzer, configFolderPath);
                            break;

                        default:
                            throw new Exception($"Unexpected ConfigState: {dependencyAnalyzer.ConfigState}");
                    }

                    return runWasSuccessful;
                }
            }
            catch (Exception e)
            {
                _logger.LogIssue(TaskExceptionIssue, e);
                return false;
            }
        }

        private bool ExecuteAnalysis(IDependencyAnalyzer dependencyAnalyzer, string configFolderPath)
        {
            var config = dependencyAnalyzer.Config;
            _logger.InfoImportance = config.InfoImportance.ToMessageImportance();

            var startTime = DateTime.Now;
            _logger.LogIssue(TaskStartedIssue, configFolderPath);

            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(SourceFilePaths, ReferencedAssemblyPaths);
            var issuesReported = ReportIllegalDependencies(illegalDependencies, config.IssueKind, config.MaxIssueCount);

            var endTime = DateTime.Now;
            _logger.LogIssue(TaskFinishedIssue, endTime - startTime);

            _logger.LogTraceMessage(GetCacheStatisticsMessage(dependencyAnalyzer));

            var errorIssueDetected = issuesReported > 0 && config.IssueKind == IssueKind.Error;
            return !errorIssueDetected;
        }

        private int ReportIllegalDependencies(IEnumerable<TypeDependency> illegalDependencies, IssueKind issueKind, int maxIssueCount)
        {
            var issueCount = 0;
            foreach (var illegalDependency in illegalDependencies)
            {
                _logger.LogIssue(IssueDefinitions.IllegalDependencyIssue, illegalDependency, issueKind, illegalDependency.SourceSegment);
                issueCount++;
            }

            if (issueCount == maxIssueCount)
                _logger.LogIssue(IssueDefinitions.TooManyIssuesIssue);

            return issueCount;
        }

        private IEnumerable<string> GetInputParameterDiagnosticMessages()
        {
            yield return $"{ProductConstants.ToolName} started with parameters:";

            yield return $"  ReferencePath[{ReferencePath.Length}]";
            foreach (var taskItem in ReferencePath)
                yield return $"    {taskItem.ItemSpec}";

            yield return $"  Compile[{Compile.Length}]";
            foreach (var taskItem in Compile)
                yield return $"    {taskItem.ItemSpec}";

            yield return $"  BaseDirectory={BaseDirectory.ItemSpec}";
        }

        private static IEnumerable<string> GetCacheStatisticsMessage(ICacheStatisticsProvider cache)
        {
            if (cache != null)
                yield return $"Cache hits: {cache.HitCount}, misses: {cache.MissCount}, efficiency (hits/all): {cache.EfficiencyPercent:P}";
        }

        private static TEnum? ParseNullable<TEnum>(string valueAsString)
            where TEnum : struct
        {
            return Enum.TryParse(valueAsString, out TEnum result)
                ? (TEnum?)result
                : null;
        }
    }
}
