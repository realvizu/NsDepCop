using Codartis.NsDepCop.Core;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Codartis.NsDepCop.Analyzer.Roslyn
{
    /// <summary>
    /// Dependency analyzer implemented with Roslyn.
    /// </summary>
    public class DependencyAnalyzer : IDependencyAnalyzer
    {
        private NsDepCopConfig _config;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="config">Config object.</param>
        public DependencyAnalyzer(NsDepCopConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Gets the name of the parser.
        /// </summary>
        public string ParserName
        {
            get { return "Roslyn"; }
        }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="baseDirectory">The full path of the base directory of the project.</param>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public IEnumerable<DependencyViolation> AnalyzeProject(
            string baseDirectory,
            IEnumerable<string> sourceFilePaths, 
            IEnumerable<string> referencedAssemblyPaths)
        {
            // Build a "csc.exe command line"-like string 
            // that contains the project parameters so Roslyn can build up a workspace.
            string projectParametersAsString = string.Format("/reference:{0} {1}",
                referencedAssemblyPaths.ToSingleString(",", "\"", "\""),
                sourceFilePaths.ToSingleString(" ", "\"", "\""));
            Debug.WriteLine(string.Format("  ProjectParametersAsString='{0}'", projectParametersAsString), Constants.TOOL_NAME);

            // Create the Roslyn workspace and select the project (there can be only one project).
            var workspace = Workspace.LoadProjectFromCommandLineArguments(
                "NsDepCopTaskProject", "C#", projectParametersAsString, baseDirectory);
            var project = workspace.CurrentSolution.Projects.First();

            // Analyse all documents in the project.
            foreach (var document in project.Documents)
            {
                var syntaxVisitor = new DependencyAnalyzerSyntaxVisitor(document.GetSemanticModel(), _config);
                var documentRootNode = document.GetSyntaxRoot() as SyntaxNode;
                if (documentRootNode != null)
                {
                    var dependencyViolationsInDocument = syntaxVisitor.Visit(documentRootNode);
                    foreach (var dependencyViolation in dependencyViolationsInDocument)
                        yield return dependencyViolation;
                }
            }
        }
    }
}
