using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory
{
    /// <summary>
    /// Dependency analyzer implemented with NRefactory.
    /// </summary>
    public class NRefactoryAnalyzerLogic : AnalyzerLogicBase
    {
        public NRefactoryAnalyzerLogic(IProjectConfig config) 
            : base(config)
        {
        }

        protected override IEnumerable<DependencyViolation> AnalyzeProjectOverride(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            IProjectContent project = new CSharpProjectContent();
            var syntaxTrees = new List<SyntaxTree>();

            // Ensure that mscorlib.dll is always included in the referenced assemblies.
            var mscorlibPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "mscorlib.dll");
            var referencedAssemblyPathList = referencedAssemblyPaths.ToList();
            if (!referencedAssemblyPathList.Contains(mscorlibPath))
            {
                referencedAssemblyPathList.Add(mscorlibPath);
            }

            // Load the referenced assemblies into the project.
            foreach (var assemblyPath in referencedAssemblyPathList)
            {
                var assembly = new CecilLoader().LoadAssemblyFile(assemblyPath);
                project = project.AddAssemblyReferences(assembly);
            }

            // Load the syntax trees of the source files into the project.
            var parser = new CSharpParser();
            foreach (var sourceFilePath in sourceFilePaths)
            {
                using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(stream))
                {
                    var syntaxTree = parser.Parse(textReader, sourceFilePath);
                    syntaxTrees.Add(syntaxTree);
                    project = project.AddOrUpdateFiles(syntaxTree.ToTypeSystem());
                }
            }

            // Analyze all syntax trees.
            var compilation = project.CreateCompilation();
            foreach (var syntaxTree in syntaxTrees)
            {
                var visitor = new DependencyAnalyzerSyntaxVisitor(compilation, syntaxTree, TypeDependencyValidator, Config.MaxIssueCount);
                syntaxTree.AcceptVisitor(visitor);

                foreach (var dependencyViolation in visitor.DependencyViolations)
                    yield return dependencyViolation;
            }
        }

        protected override IEnumerable<DependencyViolation> AnalyzeSyntaxNodeOverride(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            throw new System.NotImplementedException("Syntax node level analysis is not implemented with NRefactory.");
        }
    }
}
