using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.ParserAdapter.Implementation
{
    /// <summary>
    /// Enumerates type dependencies in source code using Roslyn 2.x
    /// </summary>
    public class Roslyn2TypeDependencyEnumerator : RoslynTypeDependencyEnumeratorBase
    {
        public Roslyn2TypeDependencyEnumerator(MessageHandler infoMessageHandler, MessageHandler diagnosticMessageHandler)
            : base(new Roslyn2SyntaxNodeAnalyzer(), infoMessageHandler, diagnosticMessageHandler)
        {
        }
    }
}
