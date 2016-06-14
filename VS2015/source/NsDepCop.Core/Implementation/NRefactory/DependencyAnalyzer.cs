using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Codartis.NsDepCop.Core.Interface;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Codartis.NsDepCop.Core.Implementation.NRefactory
{
    /// <summary>
    /// Dependency analyzer implemented with NRefactory.
    /// </summary>
    public class DependencyAnalyzer : DependencyAnalyzerBase
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="config">Config object.</param>
        public DependencyAnalyzer(NsDepCopConfig config, ITypeDependencyValidator typeDependencyValidator) 
            : base(config, typeDependencyValidator)
        {
        }

        /// <summary>
        /// Gets the name of the parser.
        /// </summary>
        public override string ParserName => "NRefactory";

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public override IEnumerable<DependencyViolation> AnalyzeProject(
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
                var visitor = new DependencyAnalyzerSyntaxVisitor(compilation, syntaxTree, Config, TypeDependencyValidator);
                syntaxTree.AcceptVisitor(visitor);

                foreach (var dependencyViolation in visitor.DependencyViolations)
                    yield return dependencyViolation;
            }

            DebugDumpCacheStatistics();
        }
    }
}
