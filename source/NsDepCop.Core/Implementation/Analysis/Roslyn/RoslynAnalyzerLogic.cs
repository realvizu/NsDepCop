using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    /// <summary>
    /// Dependency analyzer implemented with Roslyn.
    /// </summary>
    public class RoslynAnalyzerLogic : AnalyzerLogicBase
    {
        public RoslynAnalyzerLogic(IProjectConfig config)
            : base(config)
        {
        }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        protected override IEnumerable<DependencyViolation> AnalyzeProjectOverride(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            var referencedAssemblies = referencedAssemblyPaths.Select(i => MetadataReference.CreateFromFile(i)).ToList();
            var syntaxTrees = sourceFilePaths.Select(ParseFile).ToList();
            var compilation = CSharpCompilation.Create("NsDepCopTaskProject", syntaxTrees, referencedAssemblies);

            foreach (var syntaxTree in syntaxTrees)
            {
                var syntaxVisitor = new DependencyAnalyzerSyntaxVisitor(compilation.GetSemanticModel(syntaxTree), TypeDependencyValidator, Config.MaxIssueCount);
                var documentRootNode = syntaxTree.GetRoot();
                if (documentRootNode != null)
                {
                    var dependencyViolationsInDocument = syntaxVisitor.Visit(documentRootNode);
                    foreach (var dependencyViolation in dependencyViolationsInDocument)
                        yield return dependencyViolation;
                }
            }
        }

        protected override IEnumerable<DependencyViolation> AnalyzeSyntaxNodeOverride(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            return SyntaxNodeAnalyzer.Analyze(Unwrap<SyntaxNode>(syntaxNode), Unwrap<SemanticModel>(semanticModel), TypeDependencyValidator);
        }

        private static TUnwrapped Unwrap<TUnwrapped>(object wrappedValue)
        {
            if (wrappedValue == null)
                throw new ArgumentNullException(nameof(wrappedValue));

            if (!(wrappedValue is ObjectWrapper<TUnwrapped>))
                throw new ArgumentException("Wrapped value should be a subclass of ObjectWrapper<T>).");

            return ((ObjectWrapper<TUnwrapped>)wrappedValue).Value;
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
