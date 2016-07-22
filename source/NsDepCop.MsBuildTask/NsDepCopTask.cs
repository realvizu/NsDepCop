using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements a custom MsBuild task that performs namespace dependency analysis 
    /// and reports disallowed dependencies.
    /// </summary>
    public class NsDepCopTask : Task
    {
        private const ParserType DefaultParserType = ParserType.Roslyn;
        private const MessageImportance DefaultMessageImportance = MessageImportance.Normal;

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
        /// Specifies the parser: NRefactory or Roslyn. Optional. Roslyn is default if omitted or unrecognized.
        /// </summary>
        public ITaskItem Parser { get; set; }

        /// <summary>
        /// Specifies the info log events' message importance level. Optional. Normal is default if omitted or unrecognized.
        /// </summary>
        public ITaskItem InfoImportance { get; set; }

        private MessageImportance _currentMessageImportance = DefaultMessageImportance;

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
                Debug.WriteLine("Execute started...", Constants.TOOL_NAME);
                DebugDumpInputParameters();

                _currentMessageImportance = ParseMessageImportance(GetValueOfTaskItem(InfoImportance));

                var parserName = GetValueOfTaskItem(Parser);
                var configFileName = Path.Combine(BaseDirectory.ItemSpec, Constants.DEFAULT_CONFIG_FILE_NAME);

                return ExecuteAnalysis(parserName, configFileName);
            }
            catch (Exception e)
            {
                LogMsBuildEvent(TaskExceptionIssue, e);
                return false;
            }
        }

        private bool ExecuteAnalysis(string parserName, string configFileName)
        {
            var runWasSuccessful = true;
            var startTime = DateTime.Now;

            using (var dependencyAnalyzer = DependencyAnalyzerFactory.Create(parserName, configFileName, DefaultParserType))
            {
                switch (dependencyAnalyzer.State)
                {
                    case DependencyAnalyzerState.NoConfigFile:
                        LogMsBuildEvent(Constants.NoConfigFileIssue);
                        break;

                    case DependencyAnalyzerState.Disabled:
                        LogMsBuildEvent(Constants.ConfigDisabledIssue);
                        break;

                    case DependencyAnalyzerState.ConfigError:
                        LogMsBuildEvent(Constants.ConfigExceptionIssue, dependencyAnalyzer.ConfigException);
                        runWasSuccessful = false;
                        break;

                    case DependencyAnalyzerState.Enabled:
                        LogMsBuildEvent(TaskStartedIssue, dependencyAnalyzer.ParserName);
                        var errorIssueDetected = AnalyzeProjectAndReportIssues(dependencyAnalyzer);

                        runWasSuccessful = !errorIssueDetected;
                        var endTime = DateTime.Now;
                        LogMsBuildEvent(TaskFinishedIssue, endTime - startTime);
                        break;

                    default:
                        throw new Exception($"Unexpected DependencyAnalyzerState: {dependencyAnalyzer.State}");
                }
            }

            return runWasSuccessful;
        }

        private bool AnalyzeProjectAndReportIssues(IDependencyAnalyzer dependencyAnalyzer)
        {
            var dependencyViolations = dependencyAnalyzer.AnalyzeProject(SourceFilePaths, ReferencedAssemblyPaths).ToList();

            ReportIssuesToMsBuild(dependencyViolations, dependencyAnalyzer.DependencyViolationIssueKind, dependencyAnalyzer.MaxIssueCount);

            var errorIssueDetected = dependencyViolations.Any() && dependencyAnalyzer.DependencyViolationIssueKind == IssueKind.Error;
            return errorIssueDetected;
        }

        private void ReportIssuesToMsBuild(IEnumerable<DependencyViolation> dependencyViolations,
            IssueKind issueKind, int maxIssueCount)
        {
            var issuesReported = 0;
            foreach (var dependencyViolation in dependencyViolations)
            {
                LogMsBuildEvent(Constants.IllegalDependencyIssue, issueKind,
                    dependencyViolation.SourceSegment, dependencyViolation.ToString());

                issuesReported++;

                // Too many issues stop the analysis.
                if (issuesReported == maxIssueCount)
                {
                    LogMsBuildEvent(Constants.TooManyIssuesIssue);
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
            message = "[" + Constants.TOOL_NAME + "] " + message;

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
                case (IssueKind.Error):
                    BuildEngine.LogErrorEvent(new BuildErrorEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, Constants.TOOL_NAME));
                    break;

                case (IssueKind.Warning):
                    BuildEngine.LogWarningEvent(new BuildWarningEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, Constants.TOOL_NAME));
                    break;

                default:
                    BuildEngine.LogMessageEvent(new BuildMessageEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, Constants.TOOL_NAME,
                        _currentMessageImportance));
                    break;
            }
        }

        private static MessageImportance ParseMessageImportance(string infoImportanceString)
        {
            MessageImportance result;
            if (!Enum.TryParse(infoImportanceString, out result))
                result = DefaultMessageImportance;

            return result;
        }

        private static string GetValueOfTaskItem(ITaskItem taskItem)
        {
            return taskItem?.ItemSpec;
        }

        private void DebugDumpInputParameters()
        {
            Debug.WriteLine($"  ReferencePath[{ReferencePath.Length}]", Constants.TOOL_NAME);
            ReferencePath.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", Constants.TOOL_NAME));
            Debug.WriteLine($"  Compile[{Compile.Length}]", Constants.TOOL_NAME);
            Compile.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", Constants.TOOL_NAME));
            Debug.WriteLine($"  BaseDirectory={BaseDirectory.ItemSpec}", Constants.TOOL_NAME);
        }
    }
}
