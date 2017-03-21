using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    public static class SyntaxNodeExtensions
    {
        public static bool HasDescendant<T>(this SyntaxNode syntaxNode)
            where T : SyntaxNode
        {
            if (syntaxNode == null)
                throw new ArgumentNullException(nameof(syntaxNode));

            return syntaxNode.ChildNodes().Any(i => i.IsOrHasDescendant<T>());
        }

        public static bool IsOrHasDescendant<T>(this SyntaxNode syntaxNode)
            where T : SyntaxNode
        {
            if (syntaxNode == null)
                return false;

            if (syntaxNode.GetType() == typeof(T))
                return true;

            return syntaxNode.ChildNodes().Any(i => i.IsOrHasDescendant<T>());
        }

        public static bool HasParent<T>(this SyntaxNode syntaxNode)
            where T : SyntaxNode
        {
            var parent = syntaxNode.Parent;
            if (parent == null)
                return false;

            if (parent.GetType() == typeof(T))
                return true;

            return parent.HasParent<T>();
        }
    }
}
