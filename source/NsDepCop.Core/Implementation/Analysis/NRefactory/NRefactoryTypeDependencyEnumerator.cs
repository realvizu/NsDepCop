using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Codartis.NsDepCop.Core.Interface.Analysis;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory
{
    /// <summary>
    /// Finds type dependencies in source code using NRefactory as the parser.
    /// </summary>
    internal class NRefactoryTypeDependencyEnumerator : ITypeDependencyEnumerator
    {
        public IEnumerable<TypeDependency> GetTypeDependencies(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            referencedAssemblyPaths = EnsureMscorlibIsReferenced(referencedAssemblyPaths);

            var syntaxTrees = CreateSyntaxTrees(sourceFilePaths);
            var project = CreateProject(syntaxTrees, referencedAssemblyPaths);
            var compilation = project.CreateCompilation();

            foreach (var syntaxTree in syntaxTrees)
            {
                var visitor = new TypeDependencyEnumeratorSyntaxVisitor(compilation, syntaxTree);
                syntaxTree.AcceptVisitor(visitor);

                foreach (var typeDependency in visitor.TypeDependencies)
                    yield return typeDependency;
            }
        }

        public IEnumerable<TypeDependency> GetTypeDependencies(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            throw new NotImplementedException("Syntax node level type dependency finding is not implemented with NRefactory.");
        }

        private static IEnumerable<string> EnsureMscorlibIsReferenced(IEnumerable<string> referencedAssemblyPaths)
        {
            var mscorlibPath = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "mscorlib.dll");

            var referencedAssemblyPathList = referencedAssemblyPaths.ToList();
            if (!referencedAssemblyPathList.Contains(mscorlibPath))
                referencedAssemblyPathList.Add(mscorlibPath);

            return referencedAssemblyPathList;
        }

        private static List<SyntaxTree> CreateSyntaxTrees(IEnumerable<string> sourceFilePaths)
        {
            var syntaxTrees = new List<SyntaxTree>();
            var parser = new CSharpParser();
            foreach (var sourceFilePath in sourceFilePaths)
            {
                using (var stream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(stream))
                {
                    var syntaxTree = parser.Parse(textReader, sourceFilePath);
                    syntaxTrees.Add(syntaxTree);
                }
            }
            return syntaxTrees;
        }

        private static IProjectContent CreateProject(IEnumerable<SyntaxTree> syntaxTrees, IEnumerable<string> referencedAssemblyPaths)
        {
            IProjectContent project = new CSharpProjectContent();
            project = LoadReferencedAssembliesIntoProject(project, referencedAssemblyPaths);
            project = LoadSyntaxTreesIntoProject(project, syntaxTrees);
            return project;
        }

        private static IProjectContent LoadReferencedAssembliesIntoProject(IProjectContent project, IEnumerable<string> referencedAssemblyPaths)
        {
            foreach (var assemblyPath in referencedAssemblyPaths)
            {
                var assembly = new CecilLoader().LoadAssemblyFile(assemblyPath);
                project = project.AddAssemblyReferences(assembly);
            }
            return project;
        }

        private static IProjectContent LoadSyntaxTreesIntoProject(IProjectContent project, IEnumerable<SyntaxTree> syntaxTrees)
        {
            foreach (var syntaxTree in syntaxTrees)
                project = project.AddOrUpdateFiles(syntaxTree.ToTypeSystem());

            return project;
        }
    }
}
