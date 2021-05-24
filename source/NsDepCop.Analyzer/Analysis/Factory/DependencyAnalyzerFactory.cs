using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public sealed class DependencyAnalyzerFactory : IDependencyAnalyzerFactory
    {
        private readonly MessageHandler _traceMessageHandler;

        public DependencyAnalyzerFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
        }

        public IDependencyAnalyzer Create(IUpdateableConfigProvider configProvider, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
          
            return new DependencyAnalyzer(configProvider, typeDependencyEnumerator, _traceMessageHandler);
        }
    }
}
