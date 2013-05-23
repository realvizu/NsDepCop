using Codartis.NsDepCop.Core;
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
        public const string MSBUILD_CODE_ISSUE = "NSDEPCOP01";
        public const string MSBUILD_CODE_TOO_MANY_ISSUES = "NSDEPCOP02";
        public const string MSBUILD_CODE_EXCEPTION = "NSDEPCOPEX";
        public const string MSBUILD_CODE_NO_CONFIG_FILE = "NSDEPCOP03";
        public const string MSBUILD_CODE_CONFIG_DISABLED = "NSDEPCOP04";

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
        /// Executes the custom MsBuild task. Called by the MsBuild tool.
        /// </summary>
        /// <returns>True if the run was successful. False if it failed.</returns>
        public override bool Execute()
        {
            try
            {
                Debug.WriteLine("Execute started...", Constants.TOOL_NAME);
                DumpInputParametersToDebug();

                // Find out the location of the config file.
                var configFileName = Path.Combine(BaseDirectory.ItemSpec, Constants.DEFAULT_CONFIG_FILE_NAME);

                // No config file means no analysis.
                if (!File.Exists(configFileName))
                {
                    LogHelper(IssueKind.Info, MSBUILD_CODE_NO_CONFIG_FILE, 
                        Constants.TOOL_NAME + ": No config file found, analysis skipped.", null);
                    return true;
                }

                // Read the config.
                var config = new NsDepCopConfig(configFileName);

                // If analysis is switched off in the config file, then bail out.
                if (!config.IsEnabled)
                {
                    LogHelper(IssueKind.Info, MSBUILD_CODE_CONFIG_DISABLED, 
                        Constants.TOOL_NAME + ": Analysis is disabled in the nsdepcop config file.", null);
                    return true;
                }

                // Run the analysis for the whole project.
                var codeAnalyzer = new Codartis.NsDepCop.Analyzer.Roslyn.DependencyAnalyzer(config);
                var dependencyViolations = codeAnalyzer.AnalyzeProject(
                    BaseDirectory.ItemSpec, 
                    Compile.ToList().Select(i => i.ItemSpec), 
                    ReferencePath.ToList().Select(i => i.ItemSpec));

                // Report issues to MSBuild.
                var issuesReported = 0;
                foreach (var dependencyViolation in dependencyViolations)
                {
                    LogHelper(config.IssueKind, MSBUILD_CODE_ISSUE, dependencyViolation.ToString(), dependencyViolation.SourceSegment);
            
                    issuesReported++;

                    // Too many issues stop the analysis.
                    if (issuesReported == config.MaxIssueCount)
                    {
                        LogHelper(IssueKind.Warning, MSBUILD_CODE_TOO_MANY_ISSUES, 
                            "Too many NsDepCop issues, analysis was stopped.", null);
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper(IssueKind.Error, MSBUILD_CODE_EXCEPTION, string.Format(Constants.TOOL_NAME + ": {0}", e), null);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Dumps the incoming parameters to Debug output to help troubleshooting.
        /// </summary>
        private void DumpInputParametersToDebug()
        {
            Debug.WriteLine(string.Format("  ReferencePath[{0}]", ReferencePath.Length), Constants.TOOL_NAME);
            ReferencePath.ToList().ForEach(i => Debug.WriteLine(string.Format("    {0}", i.ItemSpec), Constants.TOOL_NAME));
            Debug.WriteLine(string.Format("  Compile[{0}]", Compile.Length), Constants.TOOL_NAME);
            Compile.ToList().ForEach(i => Debug.WriteLine(string.Format("    {0}", i.ItemSpec), Constants.TOOL_NAME));
            Debug.WriteLine(string.Format("  BaseDirectory={0}", BaseDirectory.ItemSpec), Constants.TOOL_NAME);
        }

        /// <summary>
        /// Log an event to MSBuild.
        /// </summary>
        /// <param name="issueKind">Error/Warning/Info</param>
        /// <param name="code">The string code of the event.</param>
        /// <param name="message">The string message of the event.</param>
        /// <param name="sourceSegment">The source segment that caused the event or null if not applicable.</param>
        private void LogHelper(IssueKind issueKind, string code, string message, SourceSegment sourceSegment)
        {
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
                        MessageImportance.High));
                    break;
            }
        }
    }
}
