using Codartis.NsDepCop.Core;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
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
            Debug.WriteLine("Execute started...", Constants.TOOL_NAME);

            try
            {
                // Dumping the incoming parameters to Debug output to help troubleshooting.
                Debug.WriteLine(string.Format("  ReferencePath[{0}]", ReferencePath.Length), Constants.TOOL_NAME);
                ReferencePath.ToList().ForEach(i => Debug.WriteLine(string.Format("    {0}", i.ItemSpec), Constants.TOOL_NAME));
                Debug.WriteLine(string.Format("  Compile[{0}]", Compile.Length), Constants.TOOL_NAME);
                Compile.ToList().ForEach(i => Debug.WriteLine(string.Format("    {0}", i.ItemSpec), Constants.TOOL_NAME));
                Debug.WriteLine(string.Format("  BaseDirectory={0}", BaseDirectory.ItemSpec), Constants.TOOL_NAME);

                // Find out the location of the config file.
                var configFileName = Path.Combine(BaseDirectory.ItemSpec, Constants.DEFAULT_CONFIG_FILE_NAME);
                
                // No config file means no analysis.
                if (!File.Exists(configFileName))
                {
                    Log.LogMessage(MessageImportance.High, Constants.TOOL_NAME + ": No config file found, analysis skipped.");
                    return true;
                }

                // Read the config.
                var config = new NsDepCopConfig(configFileName);

                // If analysis is switched off in the config file, then bail out.
                if (!config.IsEnabled)
                {
                    Log.LogMessage(MessageImportance.High, Constants.TOOL_NAME + ": Analysis is disabled in the nsdepcop config file.");
                    return true;
                }

                // Build a "csc.exe command line"-like string 
                // that contains the project parameters so Roslyn can build up a workspace.
                string projectParametersAsString = string.Format("/reference:{0} {1}",
                    ReferencePath.Select(i => i.ItemSpec).ToSingleString(",", "\"", "\""),
                    Compile.Select(i => i.ItemSpec).ToSingleString(" ", "\"", "\""));
                Debug.WriteLine(string.Format("  ProjectParametersAsString='{0}'", projectParametersAsString), Constants.TOOL_NAME);

                // Create the Roslyn workspace and select the project (there can be only on project).
                var workspace = Workspace.LoadProjectFromCommandLineArguments("NsDepCopTaskProject", "C#", 
                    projectParametersAsString, BaseDirectory.ItemSpec);
                var project = workspace.CurrentSolution.Projects.First();
                
                // Analyse all documents in the project.
                foreach (var document in project.Documents)
                {
                    Log.LogMessage(Constants.TOOL_NAME + ": Analysing document: '{0}'.", document.FilePath);

                    var syntaxWalker = new NsDepCopSyntaxWalker(document.GetSemanticModel(), config);
                    syntaxWalker.Visit(document.GetSyntaxRoot() as SyntaxNode);

                    // Log the result of the analysis.
                    var issuesReported = 0;
                    foreach (var dependencyViolation in syntaxWalker.DependencyViolations)
                    {
                        var message = dependencyViolation.ToString();
                        var lineSpan = document.GetSyntaxTree().GetLineSpan(dependencyViolation.SyntaxNode.Span, true);

                        switch (config.CodeIssueKind)
                        {
                            case (Roslyn.Services.CodeIssueKind.Error):
                                BuildEngine.LogErrorEvent(new BuildErrorEventArgs(null, Constants.CODE_ISSUE, lineSpan.Path, 
                                    lineSpan.StartLinePosition.Line+1, lineSpan.StartLinePosition.Character+1, 
                                    lineSpan.EndLinePosition.Line+1, lineSpan.EndLinePosition.Character+1,
                                    message, Constants.CODE_ISSUE, Constants.TOOL_NAME));
                                break;

                            case (Roslyn.Services.CodeIssueKind.Warning):
                                BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null, Constants.CODE_ISSUE, lineSpan.Path,
                                    lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1,
                                    lineSpan.EndLinePosition.Line + 1, lineSpan.EndLinePosition.Character + 1,
                                    message, Constants.CODE_ISSUE, Constants.TOOL_NAME));
                                break;

                            default:
                                BuildEngine.LogMessageEvent(new BuildMessageEventArgs(null, Constants.CODE_ISSUE, lineSpan.Path,
                                    lineSpan.StartLinePosition.Line + 1, lineSpan.StartLinePosition.Character + 1,
                                    lineSpan.EndLinePosition.Line + 1, lineSpan.EndLinePosition.Character + 1,
                                    message, Constants.CODE_ISSUE, Constants.TOOL_NAME, MessageImportance.High));
                                break;
                        }

                        issuesReported++;

                        // Too many issues stop the analysis.
                        if (issuesReported == Constants.MAX_ISSUE_REPORTED)
                        {
                            BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null, Constants.CODE_TOO_MANY_ISSUES, null, 0, 0, 0, 0,
                                "Too many NsDepCop issues, analysis was stopped.", Constants.CODE_TOO_MANY_ISSUES, Constants.TOOL_NAME));
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(Constants.TOOL_NAME + ": {0}", e);
                return false;
            }

            return true;
        }
    }
}
