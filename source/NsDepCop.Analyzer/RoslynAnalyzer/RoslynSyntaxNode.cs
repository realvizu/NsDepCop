using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
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
