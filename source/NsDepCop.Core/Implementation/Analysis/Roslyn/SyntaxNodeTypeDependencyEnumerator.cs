using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    /// <summary>
    /// Static helper class that enumerates type dependencies for a syntax node.
    /// </summary>
    internal static class SyntaxNodeTypeDependencyEnumerator
    {
        /// <summary>
        /// The list of those type kinds that can occur as a declaration.
        /// </summary>
        private static readonly List<TypeKind> DeclarationTypeKinds = new List<TypeKind>
        {
            TypeKind.Class,
            TypeKind.Delegate,
            TypeKind.Enum,
            TypeKind.Interface,
            TypeKind.Struct,
        };

        /// <summary>
        /// Returns type dependencies for a syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the current document.</param>
        /// <returns>A list of type dependencies. Can be empty.</returns>
        public static IEnumerable<TypeDependency> GetTypeDependencies(SyntaxNode node, SemanticModel semanticModel)
        {
            // Determine the type that contains the current syntax node.
            var enclosingType = DetermineEnclosingType(node, semanticModel);
            if (!IsAnalyzableDeclarationType(enclosingType))
                yield break;

            // Determine the type referenced by the symbol represented by the current syntax node.
            var referencedType = DetermineReferencedType(node, semanticModel);
            foreach (var type in GetConstituentTypes(referencedType, node))
                yield return CreateTypeDependency(enclosingType, type, node);

            // If this is an extension method invocation then determine the type declaring the extension method.
            var declaringType = DetermineExtensionMethodDeclaringType(node, semanticModel);
            if (IsAnalyzableDeclarationType(declaringType))
                yield return CreateTypeDependency(enclosingType, declaringType, node);
        }

        private static bool IsAnalyzableDeclarationType(ITypeSymbol typeSymbol)
        {
            return typeSymbol?.ContainingNamespace != null
                && !typeSymbol.IsAnonymousType
                && DeclarationTypeKinds.Contains(typeSymbol.TypeKind);
        }

        private static IEnumerable<ITypeSymbol> GetConstituentTypes(ITypeSymbol typeSymbol, SyntaxNode syntaxNode)
        {
            if (typeSymbol == null || 
                typeSymbol.IsAnonymousType)
                yield break;

            switch (typeSymbol.TypeKind)
            {
                case TypeKind.Array:
                    var arrayTypeSymbol = (IArrayTypeSymbol)typeSymbol;
                    foreach (var type in GetConstituentTypes(arrayTypeSymbol.ElementType, syntaxNode))
                        yield return type;
                    break;

                case TypeKind.Pointer:
                    var pointerTypeSymbol = (IPointerTypeSymbol)typeSymbol;
                    foreach (var type in GetConstituentTypes(pointerTypeSymbol.PointedAtType, syntaxNode))
                        yield return type;
                    break;

                case TypeKind.Class:
                case TypeKind.Delegate:
                case TypeKind.Enum:
                case TypeKind.Interface:
                case TypeKind.Struct:
                    var namedTypeSymbol = typeSymbol as INamedTypeSymbol;
                    if (namedTypeSymbol == null)
                        yield break;

                    if (namedTypeSymbol.IsGenericType)
                    {
                        yield return namedTypeSymbol.ConstructedFrom;

                        // If the syntax node has any identifier descendants then those will trigger analysis for the type arguments
                        // so there's no need to recurse into the type arguments here.
                        if (syntaxNode.HasDescendant<IdentifierNameSyntax>())
                            yield break;

                        foreach (var typeArgument in namedTypeSymbol.TypeArguments)
                            foreach (var type in GetConstituentTypes(typeArgument, syntaxNode))
                                yield return type;
                    }
                    else
                    {
                        yield return namedTypeSymbol;
                    }
                    break;

                case TypeKind.Dynamic:
                case TypeKind.Error:
                case TypeKind.Module:
                case TypeKind.Unknown:
                case TypeKind.Submission:
                case TypeKind.TypeParameter:
                    yield break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(typeSymbol), typeSymbol.TypeKind, "Unexpected TypeKind.");
            }
        }

        /// <summary>
        /// Returns a type dependency object for the given types.
        /// </summary>
        /// <param name="fromType">The referring type.</param>
        /// <param name="toType">The referenced type.</param>
        /// <param name="node">The syntax node currently analyzed.</param>
        /// <returns>A type dependency object.</returns>
        private static TypeDependency CreateTypeDependency(ITypeSymbol fromType, ITypeSymbol toType, SyntaxNode node)
        {
            return new TypeDependency(
                fromType.ContainingNamespace.ToDisplayString(), fromType.MetadataName,
                toType.ContainingNamespace.ToDisplayString(), toType.MetadataName,
                GetSourceSegment(node));
        }

        /// <summary>
        /// Determines the type declaring the given extension method syntax node.
        /// </summary>
        /// <param name="node">A syntax node representing an extension method.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns>The type declaring the given extension method syntax node, or null if not found.</returns>
        private static ITypeSymbol DetermineExtensionMethodDeclaringType(SyntaxNode node, SemanticModel semanticModel)
        {
            var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;

            return methodSymbol == null || !methodSymbol.IsExtensionMethod
                ? null
                : methodSymbol.ContainingType;
        }

        /// <summary>
        /// Determines the type referenced by the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns>The type referenced by the given syntax node, or null if no type was referenced.</returns>
        private static ITypeSymbol DetermineReferencedType(SyntaxNode node, SemanticModel semanticModel)
        {
            var typeSymbol = semanticModel.GetTypeInfo(node).Type;
            if (typeSymbol != null)
                return typeSymbol;

            // Special case (or Roslyn bug?): 
            // if we have an IdentifierNameSyntax inside an ObjectCreationExpression then 
            // semanticModel.GetTypeInfo(node).Type returns null but
            // semanticModel.GetSymbolInfo(node).Symbol returns the expected ITypeSymbol
            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is ITypeSymbol)
                return symbolInfo.Symbol as ITypeSymbol;

            // Special case: for method invocations we should check the return type
            if (symbolInfo.Symbol is IMethodSymbol)
                return (symbolInfo.Symbol as IMethodSymbol).ReturnType;

            // Could not determine referenced type.
            return null;
        }

        /// <summary>
        /// Determines the type that contains the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project.</param>
        /// <returns>The type that contains the given syntax node. Or null if can't determine.</returns>
        private static ITypeSymbol DetermineEnclosingType(SyntaxNode node, SemanticModel semanticModel)
        {
            // Find the type declaration that contains the current syntax node.
            var typeDeclarationSyntaxNode = node.Ancestors().FirstOrDefault(i => i is TypeDeclarationSyntax);
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
        private static SourceSegment GetSourceSegment(SyntaxNode syntaxNode)
        {
            var lineSpan = syntaxNode.GetLocation().GetLineSpan();

            return new SourceSegment
            (
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.EndLinePosition.Line + 1,
                lineSpan.EndLinePosition.Character + 1,
                syntaxNode.ToString(),
                lineSpan.Path
            );
        }
    }
}
