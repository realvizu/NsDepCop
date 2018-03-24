using Codartis.NsDepCop.Core.Implementation.Analysis.Configured;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Configured;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates objects that bundle together a dependency analyzer and its config provider.
    /// </summary>
    public sealed class ConfiguredDependencyAnalyzerFactory : 
        IConfiguredDependencyAnalyzerFactory, 
        IConfigInitializer<ConfiguredDependencyAnalyzerFactory>
    {
        private readonly MessageHandler _traceMessageHandler;
        private readonly IConfigProviderFactory _configProviderFactory;

        public ConfiguredDependencyAnalyzerFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
            _configProviderFactory = new ConfigProviderFactory(_traceMessageHandler);
        }

        public ConfiguredDependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _configProviderFactory.SetDefaultInfoImportance(defaultInfoImportance);
            return this;
        }

        public IConfiguredDependencyAnalyzer CreateInProcess(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new ConfiguredDependencyAnalyzer(configProvider, 
                () => new DependencyAnalyzerFactory(configProvider.Config, _traceMessageHandler).CreateInProcess(typeDependencyEnumerator));
        }

        public IConfiguredDependencyAnalyzer CreateOutOfProcess(string folderPath, string serviceAddress)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new ConfiguredDependencyAnalyzer(configProvider,
                () => new DependencyAnalyzerFactory(configProvider.Config, _traceMessageHandler).CreateOutOfProcess(serviceAddress));
        }
    }
}
