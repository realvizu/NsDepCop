using System.Collections.Generic;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis
{
    /// <summary>
    /// Enumerates type dependencies for a project or a syntax node.
    /// </summary>
    public interface ITypeDependencyEnumerator 
    {
        /// <summary>
        /// Enumerates type dependencies for a project (source files and referenced assemblies).
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <param name="sourcePathExclusionGlobs">A collection of file path patterns (globs) for excluding source files from analysis.</param>
        /// <returns>A collection of type dependencies.</returns>
        IEnumerable<TypeDependency> GetTypeDependencies(
            IEnumerable<string> sourceFilePaths, 
            IEnumerable<string> referencedAssemblyPaths, 
            IEnumerable<Glob> sourcePathExclusionGlobs);

        /// <summary>
        /// Enumerates type dependencies for a syntax node.
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project being analyzed.</param>
        /// <param name="sourcePathExclusionGlobs">A collection of file path patterns (globs) for excluding source files from analysis.</param>
        /// <returns>A collection of type dependencies.</returns>
        IEnumerable<TypeDependency> GetTypeDependencies(
            SyntaxNode syntaxNode, 
            SemanticModel semanticModel, 
            IEnumerable<Glob> sourcePathExclusionGlobs);
    }
}
