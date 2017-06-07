using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.ParserAdapter.Implementation
{
    /// <summary>
    /// Analyzer logic for a single syntax node.
    /// </summary>
    public interface ISyntaxNodeAnalyzer
    {
        /// <summary>
        /// Returns type dependencies for a syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the current document.</param>
        /// <returns>A list of type dependencies. Can be empty.</returns>
        IEnumerable<TypeDependency> GetTypeDependencies(SyntaxNode node, SemanticModel semanticModel);
    }
}