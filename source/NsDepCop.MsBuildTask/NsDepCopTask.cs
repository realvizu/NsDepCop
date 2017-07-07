using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Service;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements a custom MsBuild task that performs namespace dependency analysis and reports disallowed dependencies.
    /// </summary>
    /// <remarks>
    /// Invokes the analyzer from an out-of-process server via remoting 
    /// to avoid DLL version conflicts with the host process and to improve performance.
    /// </remarks>
    public class NsDepCopTask : Task
    {
        public static readonly IssueDescriptor<string> TaskStartedIssue =
            new IssueDescriptor<string>("NSDEPCOPSTART", IssueKind.Info, null, i => $"Analysing project in folder: {i}");

        public static readonly IssueDescriptor<TimeSpan> TaskFinishedIssue =
            new IssueDescriptor<TimeSpan>("NSDEPCOPFINISH", IssueKind.Info, null, i => $"Analysis took: {i:mm\\:ss\\.fff}");

        public static readonly IssueDescriptor<Exception> TaskExceptionIssue =
            new IssueDescriptor<Exception>("NSDEPCOPEX", IssueKind.Error, null, i => $"Exception during NsDepCopTask execution: {i.ToString()}");

        private const string AnalyzerServiceAddress = "ipc://NsDepCop/DependencyAnalyzerService";

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

        private ILogger _logger;

        /// <summary>
        /// Parameterless ctor is needed by MsBuild.
        /// </summary>
        public NsDepCopTask()
        {
            // Remoting loads assemblies at deserialization (even already loaded ones) 
            // and we must help it to find the NsDepCop assemblies.
            DirectoryBasedAssemblyResolver.Initialize(Assembly.GetExecutingAssembly().GetDirectory());
        }

        /// <summary>
        /// This ctor is for unit testing. 
        /// </summary>
        /// <param name="logger">A logger object.</param>
        public NsDepCopTask(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string ProjectFolder => BaseDirectory.ItemSpec;
        private IEnumerable<string> SourceFilePaths => Compile.Select(i => i.ItemSpec.ToAbsolutePath(ProjectFolder));
        private IEnumerable<string> ReferencedAssemblyPaths => ReferencePath.Select(i => i.ItemSpec);

        /// <summary>
        /// Executes the custom MsBuild task. Called by the MsBuild tool.
        /// </summary>
        /// <returns>
        /// True if there was no error and no exception.
        /// </returns>
        public override bool Execute()
        {
            if (_logger == null)
            {
                // This must not be moved to the ctor because BuildEngine is not yet inicialized at construction time.
                _logger = new MsBuildLoggerGateway(BuildEngine);
            }

            try
            {
                _logger.LogTraceMessage(GetInputParameterDiagnosticMessages());

                ServiceActivator.ActivateDependencyAnalyzerService();

                var defaultInfoImportance = EnumHelper.ParseNullable<Importance>(InfoImportance.GetValue());
                _logger.InfoImportance = defaultInfoImportance?.ToMessageImportance() ?? MessageImportance.Normal;

                var configProvider = new MultiLevelXmlFileConfigProvider(ProjectFolder, _logger.LogTraceMessage)
                    .SetDefaultInfoImportance(defaultInfoImportance);

                var runWasSuccessful = true;

                switch (configProvider.ConfigState)
                {
                    case AnalyzerConfigState.NoConfig:
                        _logger.LogIssue(IssueDefinitions.NoConfigFileIssue);
                        break;

                    case AnalyzerConfigState.Disabled:
                        _logger.LogIssue(IssueDefinitions.ConfigDisabledIssue);
                        break;

                    case AnalyzerConfigState.ConfigError:
                        _logger.LogIssue(IssueDefinitions.ConfigExceptionIssue, configProvider.ConfigException);
                        runWasSuccessful = false;
                        break;

                    case AnalyzerConfigState.Enabled:
                        runWasSuccessful = ExecuteAnalysis(configProvider.Config, ProjectFolder);
                        break;

                    default:
                        throw new Exception($"Unexpected ConfigState: {configProvider.ConfigState}");
                }

                return runWasSuccessful;
            }
            catch (Exception e)
            {
                _logger.LogIssue(TaskExceptionIssue, e);
                return false;
            }
        }

        private bool ExecuteAnalysis(IAnalyzerConfig config, string configFolderPath)
        {
            _logger.InfoImportance = config.InfoImportance.ToMessageImportance();

            var startTime = DateTime.Now;
            _logger.LogIssue(TaskStartedIssue, configFolderPath);

            var dependencyAnalyzerClient = new DependencyAnalyzerClient(AnalyzerServiceAddress);
            var analyzerMessages = dependencyAnalyzerClient.AnalyzeProject(config, SourceFilePaths.ToArray(), ReferencedAssemblyPaths.ToArray());
            var dependencyIssueCount = ReportAnalyzerMessages(analyzerMessages, config.IssueKind, config.MaxIssueCount);

            var endTime = DateTime.Now;
            _logger.LogIssue(TaskFinishedIssue, endTime - startTime);

            var errorIssueDetected = dependencyIssueCount > 0 && config.IssueKind == IssueKind.Error;
            return !errorIssueDetected;
        }

        private int ReportAnalyzerMessages(IEnumerable<AnalyzerMessageBase> analyzerMessages, IssueKind issueKind, int maxIssueCount)
        {
            var dependencyIssueCount = 0;

            foreach (var analyzerMessage in analyzerMessages)
            {
                switch (analyzerMessage)
                {
                    case IllegalDependencyMessage illegalDependencyMessage:
                        var illegalDependency = illegalDependencyMessage.IllegalDependency;
                        _logger.LogIssue(IssueDefinitions.IllegalDependencyIssue, illegalDependency, issueKind, illegalDependency.SourceSegment);
                        dependencyIssueCount++;
                        break;

                    case TraceMessage traceMessage:
                        _logger.LogTraceMessage(traceMessage.Message);
                        break;
                }
            }

            if (dependencyIssueCount == maxIssueCount)
                _logger.LogIssue(IssueDefinitions.TooManyIssuesIssue);

            return dependencyIssueCount;
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
    }
}
