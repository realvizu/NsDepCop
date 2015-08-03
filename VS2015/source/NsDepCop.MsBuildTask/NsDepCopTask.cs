﻿using Codartis.NsDepCop.Core.Analyzer.Factory;
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
        public const string MSBUILD_CODE_INFO = "NSDEPCOPINFO";
        public const string MSBUILD_CODE_EXCEPTION = "NSDEPCOPEX";
        public const IssueKind MSBUILD_CODE_EXCEPTION_SEVERITY = IssueKind.Error;

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
        /// Config info.
        /// </summary>
        private NsDepCopConfig _config;

        /// <summary>
        /// Executes the custom MsBuild task. Called by the MsBuild tool.
        /// </summary>
        /// <returns>True if the run was successful (even if issues were reported). False if the task terminated with an exception.</returns>
        public override bool Execute()
        {
            var startTime = DateTime.Now;
            var errorIssueDetected = false;

            try
            {
                Debug.WriteLine("Execute started...", Constants.TOOL_NAME);
                DebugDumpInputParameters();

                // Find out the location of the config file.
                var configFileName = Path.Combine(BaseDirectory.ItemSpec, Constants.DEFAULT_CONFIG_FILE_NAME);

                // No config file means no analysis.
                if (!File.Exists(configFileName))
                {
                    LogMsBuildEvent(Constants.DIAGNOSTIC_NOCONFIGFILE_ID, Constants.DIAGNOSTIC_NOCONFIGFILE_DESC);
                    return true;
                }

                // Read the config.
                _config = new NsDepCopConfig(configFileName);

                // If analysis is switched off in the config file, then bail out.
                if (!_config.IsEnabled)
                {
                    LogMsBuildEvent(Constants.DIAGNOSTIC_CONFIGDISABLED_ID, Constants.DIAGNOSTIC_CONFIGDISABLED_DESC);
                    return true;
                }

                // Create the code analyzer object.
                var parserName = GetValueOfTaskItem(Parser);
                var codeAnalyzer = DependencyAnalyzerFactory.Create(parserName, _config);

                LogMsBuildEvent(MSBUILD_CODE_INFO, "Analysing project using " + codeAnalyzer.ParserName + ".");

                // Run the analysis for the whole project.
                var dependencyViolations = codeAnalyzer.AnalyzeProject(
                    BaseDirectory.ItemSpec,
                    Compile.ToList().Select(i => i.ItemSpec),
                    ReferencePath.ToList().Select(i => i.ItemSpec));

                // Set return value (success indicator)
                if (dependencyViolations.Any() &&
                    GetIssueKindByCode(Constants.DIAGNOSTIC_ILLEGALDEP_ID) == IssueKind.Error)
                    errorIssueDetected = true;

                // Report issues to MSBuild.
                var issuesReported = 0;
                foreach (var dependencyViolation in dependencyViolations)
                {
                    LogMsBuildEvent(Constants.DIAGNOSTIC_ILLEGALDEP_ID, dependencyViolation.ToString(), dependencyViolation.SourceSegment);
                    issuesReported++;

                    // Too many issues stop the analysis.
                    if (issuesReported == _config.MaxIssueCount)
                    {
                        LogMsBuildEvent(Constants.DIAGNOSTIC_TOOMANYISSUES_ID, Constants.DIAGNOSTIC_TOOMANYISSUES_DESC);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                LogMsBuildEvent(MSBUILD_CODE_EXCEPTION, e.ToString());
                return false;
            }

            var endTime = DateTime.Now;
            LogMsBuildEvent(MSBUILD_CODE_INFO, string.Format("Analysis took: {0:mm\\:ss\\.fff}", endTime - startTime));

            return !errorIssueDetected;
        }

        /// <summary>
        /// Returns the string value (ItemSpec) of an MSBuild TaskItem or null if not defined.
        /// </summary>
        /// <param name="taskItem">An MSBuild TaskItem object.</param>
        /// <returns>The string value (ItemSpec) of the TaskItem or null if not defined.</returns>
        private static string GetValueOfTaskItem(ITaskItem taskItem)
        {
            return taskItem == null
                    ? null
                    : taskItem.ItemSpec;
        }

        /// <summary>
        /// Dumps the incoming parameters to Debug output to help troubleshooting.
        /// </summary>
        private void DebugDumpInputParameters()
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
        /// <param name="code">The string code of the event.</param>
        /// <param name="message">The string message of the event.</param>
        /// <param name="sourceSegment">The source segment that caused the event or null if not applicable.</param>
        private void LogMsBuildEvent(string code, string message, SourceSegment sourceSegment = null)
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

            message = "[" + Constants.TOOL_NAME + "] " + message;

            var issueKind = GetIssueKindByCode(code);

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

        /// <summary>
        /// Translates an event code to an issue kind.
        /// </summary>
        /// <param name="code">An event code.</param>
        /// <returns>The IssueKind (severity) of the given code.</returns>
        private IssueKind GetIssueKindByCode(string code)
        {
            switch (code)
            {
            case (Constants.DIAGNOSTIC_ILLEGALDEP_ID):
                return _config == null ? Constants.DIAGNOSTIC_ILLEGALDEP_DEFAULTSEVERITY : _config.IssueKind;

            case (Constants.DIAGNOSTIC_TOOMANYISSUES_ID):
                return Constants.DIAGNOSTIC_TOOMANYISSUES_DEFAULTSEVERITY;

            case (Constants.DIAGNOSTIC_CONFIGDISABLED_ID):
                return Constants.DIAGNOSTIC_CONFIGDISABLED_DEFAULTSEVERITY;

            case (Constants.DIAGNOSTIC_CONFIGEXCEPTION_ID):
                return Constants.DIAGNOSTIC_CONFIGEXCEPTION_DEFAULTSEVERITY;

            case (Constants.DIAGNOSTIC_NOCONFIGFILE_ID):
                return Constants.DIAGNOSTIC_NOCONFIGFILE_DEFAULTSEVERITY;

            case (MSBUILD_CODE_EXCEPTION):
                return MSBUILD_CODE_EXCEPTION_SEVERITY;

            default:
                return IssueKind.Info;
            }
        }
    }
}
