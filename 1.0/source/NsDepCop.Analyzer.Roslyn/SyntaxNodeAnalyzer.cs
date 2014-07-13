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
        public static DependencyViolation Analyze(CommonSyntaxNode node, ISemanticModel semanticModel, NsDepCopConfig config)
        {
            // Determine the types referenced by the symbol represented by the current syntax node.
            var referencedType = DetermineReferencedType(node, semanticModel);
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
            return new DependencyViolation(
                new Dependency(from, to),
                enclosingType.ToDisplayString(),
                referencedType.ToDisplayString(),
                GetSourceSegment(node)); 
        }

        /// <summary>
        /// Determines the type referenced by the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns>The type referenced by the given syntax node, or null if no type was referenced.</returns>
        private static ITypeSymbol DetermineReferencedType(CommonSyntaxNode node, ISemanticModel semanticModel)
        {
            var typeSymbol = semanticModel.GetTypeInfo(node).Type;

            // Special case: for an identifier that represents the name of a method, 
            // the parent member access expression's parent invocation expression gives the resulting type.
            if (typeSymbol == null
                && node.Parent is MemberAccessExpressionSyntax
                && node.Parent.Parent is InvocationExpressionSyntax)
            {
                typeSymbol = semanticModel.GetTypeInfo(node.Parent.Parent).Type;
            }

            return typeSymbol;
        }

        /// <summary>
        /// Determines the type that contains the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns>The type that contains the given syntax node. Or null if can't determine.</returns>
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
        /// Gets the source segment of the given syntax node.
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <returns>The source segment of the given syntax node.</returns>
        private static SourceSegment GetSourceSegment(CommonSyntaxNode syntaxNode)
        {
            var syntaxNodeOrToken = GetNodeOrTokenToReport(syntaxNode);

            var lineSpan = syntaxNodeOrToken.GetLocation().GetLineSpan(true);

            return new SourceSegment
            (
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.EndLinePosition.Character + 1,
                syntaxNodeOrToken.ToString(),
                lineSpan.Path
            );
        }

        /// <summary>
        /// Determine which node or token should be reported as the location of the issue.
        /// </summary>
        /// <param name="syntaxNode">The syntax node that caused the issue.</param>
        /// <returns>The node or token that should be reported as the location of the issue.</returns>
        private static CommonSyntaxNodeOrToken GetNodeOrTokenToReport(CommonSyntaxNode syntaxNode)
        {
            CommonSyntaxNodeOrToken syntaxNodeOrToken = syntaxNode;

            // For a Generic Name we should report its first token as the location.
            if (syntaxNode is GenericNameSyntax)
            {
                syntaxNodeOrToken = syntaxNode.GetFirstToken();
            }

            return syntaxNodeOrToken;
        }
    }
}
