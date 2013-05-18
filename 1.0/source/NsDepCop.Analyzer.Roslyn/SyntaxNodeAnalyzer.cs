using Codartis.NsDepCop.Core;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System.Linq;

namespace Codartis.NsDepCop.Analyzer.Roslyn
{
    /// <summary>
    /// Static helper class that implements the dependency analysis logic for a syntax node.
    /// </summary>
    public static class SyntaxNodeAnalyzer
    {
        /// <summary>
        /// Performs namespace dependency analysis for a SyntaxNode.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the current document.</param>
        /// <param name="config">Tool configuration info. Contains the allowed dependencies.</param>
        /// <returns>A DependencyViolation if an issue was found. Null if no problem.</returns>
        public static DependencyViolation ProcessSyntaxNode(
            CommonSyntaxNode node, ISemanticModel semanticModel, NsDepCopConfig config)
        {
            // Determine the types referenced by the symbol represented by the current syntax node.
            var referencedType = semanticModel.GetTypeInfo(node).Type;
            if (referencedType == null || referencedType.ContainingNamespace == null ||
                referencedType.TypeKind == CommonTypeKind.Error)
                return null;

            // Determine the type that contains the current syntax node.
            var enclosingType = DetermineEnclosingType(node, semanticModel);
            if (enclosingType == null || enclosingType.ContainingNamespace == null)
                return null;

            // Get containing namespace for the declaring and the referenced type, in string format.
            var from = enclosingType.ContainingNamespace.ToDisplayString();
            var to = referencedType.ContainingNamespace.ToDisplayString();

            // No rule needed to access the same namespace.
            if (from == to)
                return null;

            // Check the rules whether this dependency is allowed.
            if (config.IsAllowedDependency(from, to))
                return null;

            // Create a result item for a dependency violation.
            return CreateDependencyViolation(node, new Dependency(from, to), enclosingType, referencedType);
        }

        /// <summary>
        /// Determines the type that contains the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns></returns>
        private static ITypeSymbol DetermineEnclosingType(CommonSyntaxNode node, ISemanticModel semanticModel)
        {
            // Find the type declaration that contains the current syntax node.
            var typeDeclarationSyntaxNode = node.Ancestors().Where(i => i is TypeDeclarationSyntax).FirstOrDefault();
            if (typeDeclarationSyntaxNode == null)
                return null;

            // Determine the type of the type declaration that contains the current syntax node.
            return semanticModel.GetDeclaredSymbol(typeDeclarationSyntaxNode) as ITypeSymbol;
        }

        /// <summary>
        /// Creates a dependency violations object.
        /// </summary>
        /// <param name="syntaxNode">The syntax node where the violation was detected.</param>
        /// <param name="illegalDependency">The illegal namespace dependency.</param>
        /// <param name="referencingType">The referencing type.</param>
        /// <param name="referencedType">The referenced type.</param>
        /// <returns></returns>
        private static DependencyViolation CreateDependencyViolation(
            CommonSyntaxNode syntaxNode, Dependency illegalDependency, ISymbol referencingType, ISymbol referencedType)
        {
            var lineSpan = syntaxNode.GetLocation().GetLineSpan(true);

            var sourceSegment = new SourceSegment
            (
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.EndLinePosition.Character + 1,
                syntaxNode.ToString(),
                lineSpan.Path
            );

            return new DependencyViolation(illegalDependency,
                referencingType.ToDisplayString(), referencedType.ToDisplayString(), sourceSegment);
        }
    }
}
