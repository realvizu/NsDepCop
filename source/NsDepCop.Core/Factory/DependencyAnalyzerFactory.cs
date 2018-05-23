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
    public sealed class DependencyAnalyzerFactory : IDependencyAnalyzerFactory, IConfigInitializer<DependencyAnalyzerFactory>
    {
        private readonly MessageHandler _traceMessageHandler;
        private readonly IConfigProviderFactory _configProviderFactory;

        public DependencyAnalyzerFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
            _configProviderFactory = new ConfigProviderFactory(_traceMessageHandler);
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _configProviderFactory.SetDefaultInfoImportance(defaultInfoImportance);
            return this;
        }

        public IDependencyAnalyzer CreateInProcess(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new InProcessDependencyAnalyzer(configProvider, typeDependencyEnumerator, _traceMessageHandler);
        }

        public IDependencyAnalyzer CreateOutOfProcess(string folderPath, string serviceAddress)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new RemoteDependencyAnalyzerClient(configProvider, serviceAddress, _traceMessageHandler);
        }
    }
}
