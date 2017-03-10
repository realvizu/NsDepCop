using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements a custom MsBuild task that performs namespace dependency analysis 
    /// and reports disallowed dependencies.
    /// </summary>
    public class NsDepCopTask : Task
    {
        public static readonly IssueDescriptor<string> TaskStartedIssue =
            new IssueDescriptor<string>("NSDEPCOPSTART", IssueKind.Info, null, i => $"Analysing project using {i}.");

        public static readonly IssueDescriptor<TimeSpan> TaskFinishedIssue =
            new IssueDescriptor<TimeSpan>("NSDEPCOPFINISH", IssueKind.Info, null, i => $"Analysis took: {i:mm\\:ss\\.fff}");

        public static readonly IssueDescriptor<Exception> TaskExceptionIssue =
            new IssueDescriptor<Exception>("NSDEPCOPEX", IssueKind.Error, null, i => $"Exception during NsDepCopTask execution: {i.ToString()}");

        /// <summary>
        /// MsBuild task item list that contains the name and full path 
        /// of the assemblies referenced in the current csproj.
        /// </summary>
        [Required]
        public ITaskItem[] ReferencePath { get; set; }

        /// <summary>
        /// MsBuild task item list that contains the name and relative path
        /// of the source files in the current csproj.
        /// The paths are relative to the BaseDirectory.
        /// </summary>
        [Required]
        public ITaskItem[] Compile { get; set; }

        /// <summary>
        /// MsBuild task item that contains the full path of the directory of the csproj file.
        /// </summary>
        [Required]
        public ITaskItem BaseDirectory { get; set; }

        /// <summary>
        /// Specifies the parser: NRefactory or Roslyn. Optional. Default: Roslyn.
        /// </summary>
        public ITaskItem Parser { get; set; }

        /// <summary>
        /// Specifies the info log events' message importance level. Optional. Default: Normal.
        /// </summary>
        public ITaskItem InfoImportance { get; set; }

        private MessageImportance _infoImportance;

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
                LogDiagnosticMessages(GetInputParameterDiagnosticMessages());

                var infoImportance = Parse<Importance>(InfoImportance.GetValue());
                var parser = Parse<Parsers>(Parser.GetValue());
                // TODO: use these values as defaults

                var configFileName = Path.Combine(BaseDirectory.ItemSpec, ProductConstants.DefaultConfigFileName);
                using (var dependencyAnalyzer = 
                    DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName, diagnosticMessageHandler: LogDiagnosticMessage))
                {
                    var runWasSuccessful = true;

                    switch (dependencyAnalyzer.ConfigState)
                    {
                        case AnalyzerConfigState.NoConfig:
                            LogIssue(IssueDefinitions.NoConfigFileIssue);
                            break;

                        case AnalyzerConfigState.Disabled:
                            LogIssue(IssueDefinitions.ConfigDisabledIssue);
                            break;

                        case AnalyzerConfigState.ConfigError:
                            LogIssue(IssueDefinitions.ConfigExceptionIssue, dependencyAnalyzer.ConfigException);
                            runWasSuccessful = false;
                            break;

                        case AnalyzerConfigState.Enabled:
                            runWasSuccessful = ExecuteAnalysis(dependencyAnalyzer);
                            break;

                        default:
                            throw new Exception($"Unexpected ConfigState: {dependencyAnalyzer.ConfigState}");
                    }

                    return runWasSuccessful;
                }
            }
            catch (Exception e)
            {
                LogIssue(TaskExceptionIssue, e);
                return false;
            }
        }

        /// <summary>
        /// Executes dependency analysis on the project.
        /// </summary>
        /// <param name="dependencyAnalyzer">The dependency analyzer object.</param>
        /// <returns>True if the analysis was 'green', ie. no error issues found.</returns>
        private bool ExecuteAnalysis(IDependencyAnalyzer dependencyAnalyzer)
        {
            var config = dependencyAnalyzer.Config;

            _infoImportance = config.InfoImportance.ToMessageImportance();

            var startTime = DateTime.Now;
            LogIssue(TaskStartedIssue, config.Parser.ToString());

            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(SourceFilePaths, ReferencedAssemblyPaths);
            var issuesReported = ReportIllegalDependencies(illegalDependencies, config.IssueKind, config.MaxIssueCount);

            var endTime = DateTime.Now;
            LogIssue(TaskFinishedIssue, endTime - startTime);

            LogDiagnosticMessages(GetCacheStatisticsMessage(dependencyAnalyzer));

            var errorIssueDetected = issuesReported > 0 && config.IssueKind == IssueKind.Error;
            return !errorIssueDetected;
        }

        /// <summary>
        /// Reports illegal dependencies to the host.
        /// </summary>
        /// <param name="illegalDependencies">The illegal type dependencies.</param>
        /// <param name="issueKind">The severity of the illegal dependencies.</param>
        /// <param name="maxIssueCount">The max number of issues to report.</param>
        /// <returns>The number of issues reported.</returns>
        private int ReportIllegalDependencies(IEnumerable<TypeDependency> illegalDependencies, IssueKind issueKind, int maxIssueCount)
        {
            var issueCount = 0;
            foreach (var illegalDependency in illegalDependencies)
            {
                LogIssue(IssueDefinitions.IllegalDependencyIssue, illegalDependency.ToString(), issueKind, illegalDependency.SourceSegment);
                issueCount++;
            }

            if (issueCount == maxIssueCount)
                LogIssue(IssueDefinitions.TooManyIssuesIssue);

            return issueCount;
        }

        private void LogIssue<T>(IssueDescriptor<T> issueDescriptor, T messageParam = default(T))
        {
            LogIssue(issueDescriptor, issueDescriptor.GetDynamicDescription(messageParam));
        }

        private void LogIssue(IssueDescriptor issueDescriptor, string message = null, IssueKind? issueKind = null, SourceSegment sourceSegment = null)
        {
            issueKind = issueKind ?? issueDescriptor.DefaultKind;
            message = message ?? issueDescriptor.StaticDescription;

            var code = issueDescriptor.Id;
            var path = sourceSegment?.Path;
            var startLine = sourceSegment?.StartLine ?? 0;
            var startColumn = sourceSegment?.StartColumn ?? 0;
            var endLine = sourceSegment?.EndLine ?? 0;
            var endColumn = sourceSegment?.EndColumn ?? 0;

            LogBuildEvent(issueKind.Value, message, _infoImportance, code, path, startLine, startColumn, endLine, endColumn);
        }

        private void LogDiagnosticMessages(IEnumerable<string> messages)
        {
            foreach (var message in messages)
                LogDiagnosticMessage(message);
        }

        private void LogDiagnosticMessage(string message)
        {
            LogBuildEvent(IssueKind.Info, message, MessageImportance.Low);
        }

        private void LogBuildEvent(IssueKind issueKind, string message, MessageImportance messageImportance, string code = null,
            string path = null, int startLine = 0, int startColumn = 0, int endLine = 0, int endColumn = 0)
        {
            message = "[" + ProductConstants.ToolName + "] " + message;

            switch (issueKind)
            {
                case IssueKind.Error:
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName));
                    break;

                case IssueKind.Warning:
                    BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName));
                    break;

                default:
                    BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName, messageImportance));
                    break;
            }
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

        private static TEnum? Parse<TEnum>(string valueAsString)
            where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse(valueAsString, out result))
                return result;

            return null;
        }
    }
}
