using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

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

        private IProjectConfig _currentConfig;

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
                Debug.WriteLine("Execute started...", ProductConstants.ToolName);
                DebugDumpInputParameters();

                var messageImportance = ParseMessageImportance(GetValueOfTaskItem(InfoImportance));
                var parserName = GetValueOfTaskItem(Parser);
                // TODO: use these values as defaults

                var configFileName = Path.Combine(BaseDirectory.ItemSpec, ProductConstants.DefaultConfigFileName);
                using (var dependencyAnalyzer = DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName))
                {
                    _currentConfig = dependencyAnalyzer.Config;
                    return ExecuteAnalysis(dependencyAnalyzer);
                }
            }
            catch (Exception e)
            {
                LogMsBuildEvent(TaskExceptionIssue, e);
                return false;
            }
        }

        private bool ExecuteAnalysis(IDependencyAnalyzer dependencyAnalyzer)
        {
            var runWasSuccessful = true;

            switch (dependencyAnalyzer.ConfigState)
            {
                case ConfigState.NoConfigFile:
                    LogMsBuildEvent(IssueDefinitions.NoConfigFileIssue);
                    break;

                case ConfigState.Disabled:
                    LogMsBuildEvent(IssueDefinitions.ConfigDisabledIssue);
                    break;

                case ConfigState.ConfigError:
                    LogMsBuildEvent(IssueDefinitions.ConfigExceptionIssue, dependencyAnalyzer.ConfigException);
                    runWasSuccessful = false;
                    break;

                case ConfigState.Enabled:
                    var startTime = DateTime.Now;
                    var config = dependencyAnalyzer.Config;
                    LogMsBuildEvent(TaskStartedIssue, config.Parser.ToString());

                    var dependencyViolations = dependencyAnalyzer.AnalyzeProject(SourceFilePaths, ReferencedAssemblyPaths).ToList();
                    ReportIssuesToMsBuild(dependencyViolations, config.IssueKind, config.MaxIssueCount);
                    var errorIssueDetected = dependencyViolations.Any() && config.IssueKind == IssueKind.Error;

                    runWasSuccessful = !errorIssueDetected;

                    var endTime = DateTime.Now;
                    LogMsBuildEvent(TaskFinishedIssue, endTime - startTime);
                    break;

                default:
                    throw new Exception($"Unexpected ConfigState: {dependencyAnalyzer.ConfigState}");
            }

            return runWasSuccessful;
        }

        private void ReportIssuesToMsBuild(IEnumerable<DependencyViolation> dependencyViolations, IssueKind issueKind, int maxIssueCount)
        {
            var issuesReported = 0;
            foreach (var dependencyViolation in dependencyViolations)
            {
                LogMsBuildEvent(IssueDefinitions.IllegalDependencyIssue, issueKind, dependencyViolation.SourceSegment, dependencyViolation.ToString());

                issuesReported++;

                // Too many issues stop the analysis.
                if (issuesReported == maxIssueCount)
                {
                    LogMsBuildEvent(IssueDefinitions.TooManyIssuesIssue);
                    break;
                }
            }
        }

        private void LogMsBuildEvent(IssueDescriptor issueDescriptor)
        {
            LogMsBuildEvent(issueDescriptor, issueDescriptor.DefaultKind, null, null);
        }

        private void LogMsBuildEvent<T>(IssueDescriptor<T> issueDescriptor, T messageParam = default(T))
        {
            LogMsBuildEvent(issueDescriptor, issueDescriptor.DefaultKind, null, issueDescriptor.GetDynamicDescription(messageParam));
        }

        private void LogMsBuildEvent(IssueDescriptor issueDescriptor, IssueKind issueKind, SourceSegment sourceSegment, string message)
        {
            var code = issueDescriptor.Id;

            message = message ?? issueDescriptor.StaticDescription;
            message = "[" + ProductConstants.ToolName + "] " + message;

            string path = null;
            int startLine = 0;
            int startColumn = 0;
            int endLine = 0;
            int endColumn = 0;

            if (sourceSegment != null)
            {
                path = sourceSegment.Path;
                startLine = sourceSegment.StartLine;
                startColumn = sourceSegment.StartColumn;
                endLine = sourceSegment.EndLine;
                endColumn = sourceSegment.EndColumn;
            }

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
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName, ToMessageImportance(_currentConfig?.InfoImportance)));
                    break;
            }
        }

        private MessageImportance ToMessageImportance(Importance? infoImportance)
        {
            switch (infoImportance)
            {
                case Importance.Low:
                    return MessageImportance.Low;
                case Importance.High:
                    return MessageImportance.High;
                default:
                    return MessageImportance.Normal;
            }
        }

        private static MessageImportance? ParseMessageImportance(string infoImportanceString)
        {
            MessageImportance result;
            if (Enum.TryParse(infoImportanceString, out result))
                return result;

            return null;
        }

        private static string GetValueOfTaskItem(ITaskItem taskItem)
        {
            return taskItem?.ItemSpec;
        }

        private void DebugDumpInputParameters()
        {
            Debug.WriteLine($"  ReferencePath[{ReferencePath.Length}]", ProductConstants.ToolName);
            ReferencePath.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", ProductConstants.ToolName));
            Debug.WriteLine($"  Compile[{Compile.Length}]", ProductConstants.ToolName);
            Compile.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", ProductConstants.ToolName));
            Debug.WriteLine($"  BaseDirectory={BaseDirectory.ItemSpec}", ProductConstants.ToolName);
        }
    }
}
