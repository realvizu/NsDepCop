using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Factory;
using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Interface.Analysis.Messages;
using Codartis.NsDepCop.ParserAdapter.Roslyn;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.SourceTest
{
    internal sealed class SourceTestSpecification : FileBasedTestsBase
    {
        private static readonly CSharpParseOptions CSharpParseOptions = new(LanguageVersion.Latest);

        private readonly string _name;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly List<SourceLineSegment> _invalidLineSegments = new();

        private SourceTestSpecification(string name, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            _name = name;
            _typeDependencyEnumerator = typeDependencyEnumerator;
        }

        public static SourceTestSpecification Create([CallerMemberName] string name = null)
            => new(name, new TypeDependencyEnumerator(new SyntaxNodeAnalyzer(), DebugMessageHandler));

        public SourceTestSpecification ExpectInvalidSegment(int line, int startColumn, int endColumn)
        {
            _invalidLineSegments.Add(new SourceLineSegment(line, startColumn, endColumn));
            return this;
        }

        public void Execute()
        {
            var sourceFilePaths = new[] {_name}.Select(GetTestFileFullPath).ToList();
            var referencedAssemblyPaths = GetReferencedAssemblyPaths().ToList();

            ValidateCompilation(sourceFilePaths, referencedAssemblyPaths);
            AssertIllegalDependencies(sourceFilePaths, referencedAssemblyPaths);
        }

        private static void DebugMessageHandler(string message) => Debug.WriteLine(message);

        private void ValidateCompilation(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var compilation = CSharpCompilation.Create(
                "NsDepCopProject",
                sourceFiles.Select(i => CSharpSyntaxTree.ParseText(LoadFile(i), CSharpParseOptions)),
                referencedAssemblies.Select(i => MetadataReference.CreateFromFile(i)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));

            var errors = compilation.GetDiagnostics().Where(i => i.Severity == DiagnosticSeverity.Error).ToList();
            errors.Should().HaveCount(0);
        }

        private void AssertIllegalDependencies(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var baseFolder = GetBinFilePath(_name);
            var illegalDependencies = GetIllegalDependencies(baseFolder, sourceFiles, referencedAssemblies).ToList();

            illegalDependencies.Select(i => i.SourceSegment)
                .Should().Equal(_invalidLineSegments,
                    (typeDependency, sourceLineSegment) => sourceLineSegment.Equals(typeDependency));
        }

        private IEnumerable<TypeDependency> GetIllegalDependencies(string baseFolder,
            IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(DebugMessageHandler);
            var dependencyAnalyzer = dependencyAnalyzerFactory.Create(baseFolder, _typeDependencyEnumerator);
            return dependencyAnalyzer.AnalyzeProject(sourceFiles, referencedAssemblies).OfType<IllegalDependencyMessage>().Select(i => i.IllegalDependency);
        }

        private static string GetTestFileFullPath(string testName)
        {
            return Path.Combine(GetBinFilePath($@"{testName}\{testName}.cs"));
        }

        private static IEnumerable<string> GetReferencedAssemblyPaths()
        {
            return new[]
            {
                // mscorlib
                GetAssemblyPath(typeof(object).Assembly),
            };
        }
    }
}