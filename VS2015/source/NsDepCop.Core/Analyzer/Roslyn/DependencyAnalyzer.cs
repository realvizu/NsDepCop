using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Codartis.NsDepCop.Core.Analyzer.Roslyn
{
    /// <summary>
    /// Dependency analyzer implemented with Roslyn.
    /// </summary>
    public class DependencyAnalyzer : DependencyAnalyzerBase
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="config">Config object.</param>
        public DependencyAnalyzer(NsDepCopConfig config)
            : base(config)
        {
        }

        /// <summary>
        /// Gets the name of the parser.
        /// </summary>
        public override string ParserName => "Roslyn";

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="baseDirectory">The full path of the base directory of the project.</param>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public override IEnumerable<DependencyViolation> AnalyzeProject(
            string baseDirectory,
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            var referencedAssemblies = referencedAssemblyPaths.Select(i => MetadataReference.CreateFromFile(i)).ToList();
            var syntaxTrees = sourceFilePaths.Select(ParseFile).ToList();
            var compilation = CSharpCompilation.Create("NsDepCopTaskProject", syntaxTrees, referencedAssemblies);

            foreach (var syntaxTree in syntaxTrees)
            {
                var syntaxVisitor = new DependencyAnalyzerSyntaxVisitor(compilation.GetSemanticModel(syntaxTree), Config, TypeDependencyValidator);
                var documentRootNode = syntaxTree.GetRoot();
                if (documentRootNode != null)
                {
                    var dependencyViolationsInDocument = syntaxVisitor.Visit(documentRootNode);
                    foreach (var dependencyViolation in dependencyViolationsInDocument)
                        yield return dependencyViolation;
                }
            }

            DebugDumpCacheStatistics();
        }

        private static SyntaxTree ParseFile(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(stream))
            {
                var sourceText = streamReader.ReadToEnd();
                return CSharpSyntaxTree.ParseText(sourceText, null, fileName);
            }
        }
    }
}
