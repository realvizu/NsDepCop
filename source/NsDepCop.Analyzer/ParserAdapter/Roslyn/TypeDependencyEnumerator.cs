using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Util;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.ParserAdapter.Roslyn
{
    /// <summary>
    /// Implements a type dependency enumerators that use Roslyn as the parser.
    /// </summary>
    public class TypeDependencyEnumerator : ITypeDependencyEnumerator
    {
        private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

        private readonly ISyntaxNodeAnalyzer _syntaxNodeAnalyzer;
        private readonly MessageHandler _traceMessageHandler;

        public TypeDependencyEnumerator(ISyntaxNodeAnalyzer syntaxNodeAnalyzer, MessageHandler traceMessageHandler)
        {
            _syntaxNodeAnalyzer = syntaxNodeAnalyzer ?? throw new ArgumentNullException(nameof(syntaxNodeAnalyzer));
            _traceMessageHandler = traceMessageHandler;
        }

        private static TypeDependencyEnumeratorSyntaxVisitor CreateSyntaxVisitor(
            SemanticModel semanticModel,
            ISyntaxNodeAnalyzer syntaxNodeAnalyzer)
        {
            return new(semanticModel, syntaxNodeAnalyzer);
        }

        public IEnumerable<TypeDependency> GetTypeDependencies(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths,
            IEnumerable<Glob> sourcePathExclusionGlobs)
        {
            var referencedAssemblies = referencedAssemblyPaths.Select(LoadMetadata).Where(i => i != null).ToList();
            var syntaxTrees = sourceFilePaths.Select(ParseFile).Where(i => i != null).ToList();

            var compilation = CSharpCompilation.Create("NsDepCopProject", syntaxTrees, referencedAssemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            return syntaxTrees.Where(i => !IsExcludedFilePath(i.FilePath, sourcePathExclusionGlobs))
                .SelectMany(i => GetTypeDependenciesForSyntaxTree(compilation, i, _syntaxNodeAnalyzer));
        }

        private static bool IsExcludedFilePath(string filePath, IEnumerable<Glob> sourcePathExclusionGlobs)
        {
            return sourcePathExclusionGlobs.Any(i => i.IsMatch(filePath));
        }

        private static IEnumerable<TypeDependency> GetTypeDependenciesForSyntaxTree(
            Compilation compilation,
            SyntaxTree syntaxTree,
            ISyntaxNodeAnalyzer syntaxNodeAnalyzer)
        {
            var documentRootNode = syntaxTree.GetRoot();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var syntaxVisitor = CreateSyntaxVisitor(semanticModel, syntaxNodeAnalyzer);
            syntaxVisitor.Visit(documentRootNode);
            return syntaxVisitor.TypeDependencies;
        }

        public IEnumerable<TypeDependency> GetTypeDependencies(
            SyntaxNode syntaxNode,
            SemanticModel semanticModel,
            IEnumerable<Glob> sourcePathExclusionGlobs)
        {
            return IsExcludedFilePath(syntaxNode?.SyntaxTree.FilePath, sourcePathExclusionGlobs)
                ? Enumerable.Empty<TypeDependency>()
                : _syntaxNodeAnalyzer.GetTypeDependencies(syntaxNode, semanticModel);
        }

  
        private MetadataReference LoadMetadata(string fileName)
        {
            try
            {
                return MetadataReference.CreateFromFile(fileName);
            }
            catch (Exception e)
            {
                LogTraceMessage($"Error loading metadata file '{fileName}': {e}");
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
                    return CSharpSyntaxTree.ParseText(sourceText, ParseOptions, fileName);
                }
            }
            catch (Exception e)
            {
                LogTraceMessage($"Error parsing source file '{fileName}': {e}");
                return null;
            }
        }

        private void LogTraceMessage(string message) => _traceMessageHandler?.Invoke(message);
    }
}