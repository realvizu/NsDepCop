using Codartis.NsDepCop.Core.Implementation.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;

namespace Codartis.NsDepCop.ServiceHost
{
    /// <summary>
    /// Implements a dependency analyzer as a remoting server. 
    /// </summary>
    public sealed class RemoteDependencyAnalyzerServer : RemoteDependencyAnalyzerServerBase
    {
        protected override ITypeDependencyEnumerator GetTypeDependencyEnumerator(MessageHandler traceMessageHandler) 
            => new Roslyn2TypeDependencyEnumerator(traceMessageHandler);
    }
}
