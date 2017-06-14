using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.ParserAdapter.Roslyn2x
{
    /// <summary>
    /// Enumerates type dependencies in source code using Roslyn 2.x
    /// </summary>
    public class Roslyn2TypeDependencyEnumerator : RoslynTypeDependencyEnumeratorBase
    {
        public Roslyn2TypeDependencyEnumerator(MessageHandler traceMessageHandler)
            : base(new Roslyn2SyntaxNodeAnalyzer(), traceMessageHandler)
        {
        }
    }
}
