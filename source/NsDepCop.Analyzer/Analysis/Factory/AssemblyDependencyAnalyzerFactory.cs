using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Factory
{
    public sealed class AssemblyDependencyAnalyzerFactory : IAssemblyDependencyAnalyzerFactory
    {
        private readonly MessageHandler _traceMessageHandler;

        public AssemblyDependencyAnalyzerFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
        }

        public IAssemblyDependencyAnalyzer Create(IUpdateableConfigProvider configProvider)
        {
          
            return new AssemblyDependencyAnalyzer(configProvider, _traceMessageHandler);
        }
    }
}
