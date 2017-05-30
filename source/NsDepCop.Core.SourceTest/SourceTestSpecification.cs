using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.Core.SourceTest
{
    internal class SourceTestSpecification : FileBasedTestsBase
    {
        private readonly string _name;
        private readonly List<SourceLineSegment> _invalidLineSegments = new List<SourceLineSegment>();

        private SourceTestSpecification(string name)
        {
            _name = name;
        }

        public static SourceTestSpecification Create(string name) => new SourceTestSpecification(name);

        public SourceTestSpecification ExpectEnvalidSegment(int line, int startColumn, int endColumn)
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
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var errors = compilation.GetDiagnostics().Where(i => i.Severity == DiagnosticSeverity.Error).ToList();
            errors.Should().HaveCount(0);
        }

        private void AssertIllegalDependencies(IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var baseFolder = GetBinFilePath(_name);
            var illegalDependencies = GetIllegalDependencies(baseFolder, sourceFiles, referencedAssemblies).ToList();

            illegalDependencies.Should().HaveCount(_invalidLineSegments.Count);

            foreach (var illegalDependency in illegalDependencies)
                _invalidLineSegments.Should().Contain(i => i.Equals(illegalDependency.SourceSegment));
        }

        private static IEnumerable<TypeDependency> GetIllegalDependencies(string baseFolder,
            IEnumerable<string> sourceFiles, IEnumerable<string> referencedAssemblies)
        {
            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(Console.WriteLine, Console.WriteLine);
            var dependencyAnalyzer = dependencyAnalyzerFactory.CreateFromMultiLevelXmlConfigFile(baseFolder);
            return dependencyAnalyzer.AnalyzeProject(sourceFiles, referencedAssemblies);
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
