using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    /// <summary>
    /// Finds type dependencies in source code using Roslyn as the parser.
    /// </summary>
    internal class RoslynTypeDependencyEnumerator : ITypeDependencyEnumerator
    {
        public IEnumerable<TypeDependency> GetTypeDependencies(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var referencedAssemblies = referencedAssemblyPaths.Select(i => MetadataReference.CreateFromFile(i)).ToList();
            var syntaxTrees = sourceFilePaths.Select(ParseFile).ToList();
            var compilation = CSharpCompilation.Create("NsDepCopProject", syntaxTrees, referencedAssemblies);

            foreach (var syntaxTree in syntaxTrees)
            {
                var documentRootNode = syntaxTree.GetRoot();
                if (documentRootNode != null)
                {
                    var syntaxVisitor = new TypeDependencyEnumeratorSyntaxVisitor(compilation.GetSemanticModel(syntaxTree));
                    syntaxVisitor.Visit(documentRootNode);

                    foreach (var typeDependency in syntaxVisitor.TypeDependencies)
                        yield return typeDependency;
                }
            }
        }

        public IEnumerable<TypeDependency> GetTypeDependencies(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            return SyntaxNodeTypeDependencyEnumerator.GetTypeDependencies(Unwrap<SyntaxNode>(syntaxNode), Unwrap<SemanticModel>(semanticModel));
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
