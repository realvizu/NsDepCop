using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.SourceTest
{
    internal class SourceTestSpecification : FileBasedTestsBase
    {
        private readonly string _name;
        private readonly List<SourceLineSegment> _invalidLineSegments = new List<SourceLineSegment>();

        private SourceTestSpecification(string name)
        {
            _name = name;
        }

        public static SourceTestSpecification Create([CallerMemberName] string name = null) => new SourceTestSpecification(name);

        public SourceTestSpecification ExpectInvalidSegment(int line, int startColumn, int endColumn)
        {
            _invalidLineSegments.Add(new SourceLineSegment(line, startColumn, endColumn));
            return this;
        }

        public void Execute()
        {
            var sourceFilePaths = new[] { _name }.Select(GetTestFileFullPath).ToList();
            var referencedAssemblyPaths = GetReferencedAssemblyPaths().ToList();

            ValidateCompilation(sourceFilePaths, referencedAssemblyPaths);
            AssertIllegalDependencies(sourceFilePaths, referencedAssemblyPaths);
        }

        private void ValidateCompilation(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var compilation = CSharpCompilation.Create(
                "NsDepCopProject",
                sourceFiles.Select(i => CSharpSyntaxTree.ParseText(LoadFile(i))),
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

        private static IEnumerable<TypeDependency> GetIllegalDependencies(string baseFolder,
            IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(DebugMessageHandler, DebugMessageHandler);
            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(typeDependencyEnumerator, DebugMessageHandler, DebugMessageHandler);
            var dependencyAnalyzer = dependencyAnalyzerFactory.CreateFromMultiLevelXmlConfigFile(baseFolder);
            return dependencyAnalyzer.AnalyzeProject(sourceFiles, referencedAssemblies);
        }

        private static void DebugMessageHandler(string i) => Debug.WriteLine(i);

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
