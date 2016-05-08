using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public override IEnumerable<DependencyViolation> AnalyzeProject(
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

        public IEnumerable<DependencyViolation> AnalyzeNode(SyntaxNode node, SemanticModel semanticModel)
        {
            return SyntaxNodeAnalyzer.Analyze(node, semanticModel, TypeDependencyValidator);
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
