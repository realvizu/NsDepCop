using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Defines the responsibilities of an analyzer that finds dependency violations at project level.
    /// </summary>
    public interface IDependencyAnalyzer 
    {
        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        IEnumerable<DependencyViolation> AnalyzeProject(
            IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);

        /// <summary>
        /// Analyzes a syntax node.
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project being analyzed.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        IEnumerable<DependencyViolation> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel);
    }
}
