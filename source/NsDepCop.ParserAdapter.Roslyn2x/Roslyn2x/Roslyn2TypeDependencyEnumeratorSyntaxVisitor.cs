using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.ParserAdapter.Roslyn2x
{
    /// <summary>
    /// Implements a syntax visitor for Roslyn 2.x that traverses the syntax tree and finds all type dependencies. 
    /// </summary>
    public class Roslyn2TypeDependencyEnumeratorSyntaxVisitor : TypeDependencyEnumeratorSyntaxVisitor
    {
        public Roslyn2TypeDependencyEnumeratorSyntaxVisitor(SemanticModel semanticModel, ISyntaxNodeAnalyzer syntaxNodeAnalyzer)
            : base(semanticModel, syntaxNodeAnalyzer)
        {
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.Kind() == SyntaxKind.DefaultLiteralExpression)
                AnalyzeNode(node);
        }
    }
}