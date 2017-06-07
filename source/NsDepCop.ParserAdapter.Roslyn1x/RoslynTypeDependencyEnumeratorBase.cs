using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.ParserAdapter
{
    /// <summary>
    /// Abstract base class for type dependency enumerators that use Roslyn as the parser.
    /// </summary>
    public abstract class RoslynTypeDependencyEnumeratorBase : ITypeDependencyEnumerator
    {
        private readonly ISyntaxNodeAnalyzer _syntaxNodeAnalyzer;
        private readonly MessageHandler _infoMessageHandler;
        private readonly MessageHandler _diagnosticMessageHandler;

        protected RoslynTypeDependencyEnumeratorBase(ISyntaxNodeAnalyzer syntaxNodeAnalyzer,
            MessageHandler infoMessageHandler, MessageHandler diagnosticMessageHandler)
        {
            if (syntaxNodeAnalyzer == null)
                throw new ArgumentNullException(nameof(syntaxNodeAnalyzer));

            _syntaxNodeAnalyzer = syntaxNodeAnalyzer;
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
                    var syntaxVisitor = new TypeDependencyEnumeratorSyntaxVisitor(compilation.GetSemanticModel(syntaxTree), _syntaxNodeAnalyzer);
                    syntaxVisitor.Visit(documentRootNode);

                    foreach (var typeDependency in syntaxVisitor.TypeDependencies)
                        yield return typeDependency;
                }
            }
        }
        
        public IEnumerable<TypeDependency> GetTypeDependencies(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            return _syntaxNodeAnalyzer.GetTypeDependencies(Unwrap<SyntaxNode>(syntaxNode), Unwrap<SemanticModel>(semanticModel));
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
