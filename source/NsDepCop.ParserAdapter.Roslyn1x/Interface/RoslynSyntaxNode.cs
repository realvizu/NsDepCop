using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.ParserAdapter.Interface
{
    /// <summary>
    /// Wraps a Roslyn syntax node.
    /// </summary>
    public class RoslynSyntaxNode : ObjectWrapper<SyntaxNode>, ISyntaxNode
    {
        public RoslynSyntaxNode(SyntaxNode value)
            : base(value)
        {
        }
    }
}
