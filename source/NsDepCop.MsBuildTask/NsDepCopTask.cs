using Codartis.NsDepCop.Core.Analyzer.Factory;
using Codartis.NsDepCop.Core.Common;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        public static readonly IssueDescriptor TaskStartedIssue =
            new IssueDescriptor("NSDEPCOPSTART", IssueKind.Info, null, "Analysing project using {0}.");

        public static readonly IssueDescriptor TaskFinishedIssue =
            new IssueDescriptor("NSDEPCOPFINISH", IssueKind.Info, null, "Analysis took: {0:mm\\:ss\\.fff}");

        public static readonly IssueDescriptor TaskExceptionIssue =
            new IssueDescriptor("NSDEPCOPEX", IssueKind.Error, null, "Exception during NsDepCopTask execution: {0}");

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

        private NsDepCopConfig _config;
        private MessageImportance _currentMessageImportance = DefaultMessageImportance;

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

                var startTime = DateTime.Now;
                var errorIssueDetected = false;
                _currentMessageImportance = ParseMessageImportance(GetValueOfTaskItem(InfoImportance));

                // Find out the location of the config file.
                var configFileName = Path.Combine(BaseDirectory.ItemSpec, Constants.DEFAULT_CONFIG_FILE_NAME);

                // No config file means no analysis.
                if (!File.Exists(configFileName))
                {
                    LogMsBuildEvent(Constants.NoConfigFileIssue);
                    return true;
                }

                // Read the config.
                try
                {
                    _config = new NsDepCopConfig(configFileName);
                }
                catch (Exception e)
                {
                    LogMsBuildEvent(Constants.ConfigExceptionIssue, e.Message);
                    return false;
                }

                // If analysis is switched off in the config file, then bail out.
                if (!_config.IsEnabled)
                {
                    LogMsBuildEvent(Constants.ConfigDisabledIssue);
                    return true;
                }

                // Create the code analyzer object.
                var parserName = GetValueOfTaskItem(Parser);
                var codeAnalyzer = DependencyAnalyzerFactory.Create(parserName, _config, DefaultParserType);

                LogMsBuildEvent(TaskStartedIssue, codeAnalyzer.ParserName);

                // Run the analysis for the whole project.
                var dependencyViolations = codeAnalyzer.AnalyzeProject(
                    BaseDirectory.ItemSpec,
                    Compile.ToList().Select(i => i.ItemSpec),
                    ReferencePath.ToList().Select(i => i.ItemSpec)).ToList();

                // Set return value (success indicator)
                if (dependencyViolations.Any() && _config.IssueKind == IssueKind.Error)
                    errorIssueDetected = true;

                // Report issues to MSBuild.
                var issuesReported = 0;
                foreach (var dependencyViolation in dependencyViolations)
                {
                    LogMsBuildEvent(Constants.IllegalDependencyIssue, _config.IssueKind,
                        dependencyViolation.SourceSegment, dependencyViolation.ToString());

                    issuesReported++;

                    // Too many issues stop the analysis.
                    if (issuesReported == _config.MaxIssueCount)
                    {
                        LogMsBuildEvent(Constants.TooManyIssuesIssue);
                        break;
                    }
                }

                var endTime = DateTime.Now;
                LogMsBuildEvent(TaskFinishedIssue, endTime - startTime);

                return !errorIssueDetected;
            }
            catch (Exception e)
            {
                LogMsBuildEvent(TaskExceptionIssue, e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Returns the string value (ItemSpec) of an MSBuild TaskItem or null if not defined.
        /// </summary>
        /// <param name="taskItem">An MSBuild TaskItem object.</param>
        /// <returns>The string value (ItemSpec) of the TaskItem or null if not defined.</returns>
        private static string GetValueOfTaskItem(ITaskItem taskItem)
        {
            return taskItem?.ItemSpec;
        }

        /// <summary>
        /// Dumps the incoming parameters to Debug output to help troubleshooting.
        /// </summary>
        private void DebugDumpInputParameters()
        {
            Debug.WriteLine($"  ReferencePath[{ReferencePath.Length}]", Constants.TOOL_NAME);
            ReferencePath.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", Constants.TOOL_NAME));
            Debug.WriteLine($"  Compile[{Compile.Length}]", Constants.TOOL_NAME);
            Compile.ToList().ForEach(i => Debug.WriteLine($"    {i.ItemSpec}", Constants.TOOL_NAME));
            Debug.WriteLine($"  BaseDirectory={BaseDirectory.ItemSpec}", Constants.TOOL_NAME);
        }

        private void LogMsBuildEvent(IssueDescriptor issueDescriptor, params object[] messageParams)
        {
            var message = issueDescriptor.MessageFormat == null
                ? null
                : string.Format(issueDescriptor.MessageFormat, messageParams);

            LogMsBuildEvent(issueDescriptor, issueDescriptor.DefaultKind, null, message);
        }

        private void LogMsBuildEvent(IssueDescriptor issueDescriptor, IssueKind issueKind, SourceSegment sourceSegment, string message)
        {
            var code = issueDescriptor.Id;

            message = message ?? issueDescriptor.Description;
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
    }
}
