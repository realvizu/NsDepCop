using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public class DependencyAnalyzerFactory
    {
        private readonly IAnalyzerConfig _config;
        private readonly MessageHandler _traceMessageHandler;

        public DependencyAnalyzerFactory(IAnalyzerConfig config, MessageHandler traceMessageHandler)
        {
            _config = config;
            _traceMessageHandler = traceMessageHandler;
        }

        public IDependencyAnalyzer CreateInProcess(ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            return new DependencyAnalyzer(_config, typeDependencyEnumerator, _traceMessageHandler);
        }

        public IDependencyAnalyzer CreateRemote(string serviceAddress)
        {
            return new RemoteDependencyAnalyzerClient(_config, serviceAddress, _traceMessageHandler);
        }
    }
}
