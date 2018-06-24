using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.ParserAdapter.Roslyn2x
{
    /// <summary>
    /// Analyzes a syntax node using Roslyn 2.x. 
    /// Extends the logic of the Roslyn 1.x-specific syntax node analyzer
    /// </summary>
    public class Roslyn2SyntaxNodeAnalyzer : SyntaxNodeAnalyzer
    {
        protected override IEnumerable<ITypeSymbol> GetConstituentTypes(ITypeSymbol typeSymbol, SyntaxNode syntaxNode)
        {
            if (typeSymbol != null &&
                typeSymbol.TypeKind == TypeKind.Struct &&
                typeSymbol is INamedTypeSymbol namedTypeSymbol &&
                namedTypeSymbol.IsTupleType)
            {
                foreach (var tupleElementType in namedTypeSymbol.TupleElements.Select(i => i.Type))
                    foreach (var type in GetConstituentTypes(tupleElementType, syntaxNode))
                        yield return type;
            }

            foreach (var constituentType in base.GetConstituentTypes(typeSymbol, syntaxNode))
                yield return constituentType;
        }

        protected override ITypeSymbol DetermineReferencedType(SyntaxNode node, SemanticModel semanticModel)
        {
            var typeSymbol = semanticModel.GetTypeInfo(node).Type;

            // Special case: deconstructing declaration with var outside, e.g.: var (d, e) = Method2();
            if (typeSymbol == null &&
                node.Parent is DeclarationExpressionSyntax &&
                node.Parent.ChildNodes().Any(i => i is ParenthesizedVariableDesignationSyntax))
                return DetermineReferencedType(node.Parent, semanticModel);

            return base.DetermineReferencedType(node, semanticModel);
        }
    }
}
