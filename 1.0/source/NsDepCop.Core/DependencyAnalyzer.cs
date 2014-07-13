using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System.Linq;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Static helper class that implements the dependency analysis logic.
    /// </summary>
    public static class DependencyAnalyzer
    {
        /// <summary>
        /// Performs namespace dependency analysis for a SyntaxNode.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the current document.</param>
        /// <param name="config">Tool configuration info. Contains the allowed dependencies.</param>
        /// <returns>A DependencyViolation object if a violation is found. Null otherwise.</returns>
        public static DependencyViolation ProcessSyntaxNode(CommonSyntaxNode node, ISemanticModel semanticModel, NsDepCopConfig config)
        {
            // Determine the type of the symbol represented by the current syntex node.
            var referencedType = semanticModel.GetTypeInfo(node).Type;
            if (referencedType == null 
                || referencedType.TypeKind == CommonTypeKind.Error 
                || referencedType.IsAnonymousType
                || referencedType.ContainingNamespace == null )
                return null;

            // Find the type declaration that contains the current syntax node.
            var typeDeclarationSyntaxNode = node.Ancestors().Where(i => i is TypeDeclarationSyntax).FirstOrDefault();
            if (typeDeclarationSyntaxNode == null)
                return null;

            // Determine the type of the type declaration that contains the current syntax node.
            var declaringType = semanticModel.GetDeclaredSymbol(typeDeclarationSyntaxNode);
            if (declaringType == null || declaringType.ContainingNamespace == null || declaringType.ContainingNamespace.IsGlobalNamespace)
                return null;

            // Get containing namespace of both the declaring and the referenced type, in string format.
            var from = declaringType.ContainingNamespace.ToDisplayString();
            var to = referencedType.ContainingNamespace.ToDisplayString();

            // No rule needed to access the same namespace.
            if (from == to)
                return null;

            // Check the rule set whether this dependency is allowed.
            if (config.IsAllowedDependency(from, to))
                return null;

            // Return all the info about a dependency violation.
            return new DependencyViolation(node, new Dependency(from, to), declaringType, referencedType);
        }
    }
}
