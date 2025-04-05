using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Factory;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config.Factory;
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
        private readonly List<string[]> _allowedMemberNames = new();

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

        public SourceTestSpecification ExpectInvalidSegment(int line, int startColumn, int endColumn, string[] expectedAllowedMemberNames)
        {
            _invalidLineSegments.Add(new SourceLineSegment(line, startColumn, endColumn));
            _allowedMemberNames.Add(expectedAllowedMemberNames);
            return this;
        }

        public void Execute(OutputKind? outputKind = null)
        {
            var sourceFilePaths = new[] {_name}.Select(GetTestFileFullPath).ToList();
            var referencedAssemblyPaths = GetReferencedAssemblyPaths().ToList();

            ValidateCompilation(sourceFilePaths, referencedAssemblyPaths, outputKind ?? OutputKind.DynamicallyLinkedLibrary);
            AssertIllegalDependencies(sourceFilePaths, referencedAssemblyPaths);
        }

        private static void DebugMessageHandler(string message) => Debug.WriteLine(message);

        private void ValidateCompilation(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary)
        {
            var compilation = CSharpCompilation.Create(
                "NsDepCopProject",
                sourceFiles.Select(i => CSharpSyntaxTree.ParseText(LoadFile(i), CSharpParseOptions)),
                referencedAssemblies.Select(i => MetadataReference.CreateFromFile(i)),
                new CSharpCompilationOptions(outputKind, allowUnsafe: true));

            var errors = compilation.GetDiagnostics().Where(i => i.Severity == DiagnosticSeverity.Error).ToList();
            errors.Should().HaveCount(0);
        }

        private void AssertIllegalDependencies(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var baseFolder = GetBinFilePath(_name);
            var illegalDependencies = GetIllegalDependencies(baseFolder, sourceFiles, referencedAssemblies).ToList();

            illegalDependencies.Select(i => i.IllegalDependency.SourceSegment)
                .Should().Equal(_invalidLineSegments,
                    (typeDependency, sourceLineSegment) => sourceLineSegment.Equals(typeDependency));

            List<string[]> membersFromGenerator = illegalDependencies
                .Select(i => i.AllowedMemberNames)
                .Where(amn => amn.Any())
                .ToList();

            membersFromGenerator.Count.Should().Be(_allowedMemberNames.Count);

            for (int i = 0; i < membersFromGenerator.Count; i++)
            {
                membersFromGenerator[i].Should().BeEquivalentTo(_allowedMemberNames[i]);
            }
        }

        private IEnumerable<IllegalDependencyMessage> GetIllegalDependencies(string baseFolder,
            IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(DebugMessageHandler);
            var configProvider = new ConfigProviderFactory(DebugMessageHandler).CreateFromMultiLevelXmlConfigFile(baseFolder);
            var dependencyAnalyzer = dependencyAnalyzerFactory.Create(configProvider, _typeDependencyEnumerator);
            return dependencyAnalyzer.AnalyzeProject(sourceFiles, referencedAssemblies).OfType<IllegalDependencyMessage>();
        }

        private static string GetTestFileFullPath(string testName)
        {
            var relativeTestFilePath = Path.Combine(testName, testName + ".cs");
            return Path.Combine(GetBinFilePath(relativeTestFilePath));
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