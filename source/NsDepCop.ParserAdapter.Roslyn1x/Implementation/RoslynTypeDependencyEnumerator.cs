using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.ParserAdapter.Implementation
{
    /// <summary>
    /// Finds type dependencies in source code using Roslyn as the parser.
    /// </summary>
    public class RoslynTypeDependencyEnumerator : ITypeDependencyEnumerator
    {
        private readonly MessageHandler _infoMessageHandler;
        private readonly MessageHandler _diagnosticMessageHandler;

        public RoslynTypeDependencyEnumerator(MessageHandler infoMessageHandler, MessageHandler diagnosticMessageHandler)
        {
            _infoMessageHandler = infoMessageHandler;
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public IEnumerable<TypeDependency> GetTypeDependencies(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var referencedAssemblies = referencedAssemblyPaths.Select(LoadMetadata).Where(i => i != null).ToList();
            var syntaxTrees = sourceFilePaths.Select(ParseFile).Where(i => i != null).ToList();

            var compilation = CSharpCompilation.Create("NsDepCopProject", syntaxTrees, referencedAssemblies, 
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

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

        private MetadataReference LoadMetadata(string fileName)
        {
            try
            {
                return MetadataReference.CreateFromFile(fileName);
            }
            catch (Exception e)
            {
                _infoMessageHandler?.Invoke($"Error loading metadata file '{fileName}': {e}");
                return null;
            }
        }

        private SyntaxTree ParseFile(string fileName)
        {
            try
            {
                using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var streamReader = new StreamReader(stream))
                {
                    var sourceText = streamReader.ReadToEnd();
                    return CSharpSyntaxTree.ParseText(sourceText, null, fileName);
                }
            }
            catch (Exception e)
            {
                _infoMessageHandler?.Invoke($"Error parsing source file '{fileName}': {e}");
                return null;
            }
        }
    }
}
