using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Interface.Analysis
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
        /// <returns>A collection of type dependencies.</returns>
        IEnumerable<TypeDependency> GetTypeDependencies(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);

        /// <summary>
        /// Enumerates type dependencies for a syntax node.
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project being analyzed.</param>
        /// <returns>A collection of type dependencies.</returns>
        IEnumerable<TypeDependency> GetTypeDependencies(ISyntaxNode syntaxNode, ISemanticModel semanticModel);
    }
}
