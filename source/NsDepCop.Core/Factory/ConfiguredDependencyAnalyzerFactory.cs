using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates configured dependency analyzer objects.
    /// </summary>
    public sealed class ConfiguredDependencyAnalyzerFactory : 
        IConfiguredDependencyAnalyzerFactory, 
        IConfigInitializer<ConfiguredDependencyAnalyzerFactory>
    {
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly MessageHandler _traceMessageHandler;
        private readonly IConfigProviderFactory _configProviderFactory;

        public ConfiguredDependencyAnalyzerFactory(ITypeDependencyEnumerator typeDependencyEnumerator, MessageHandler traceMessageHandler)
        {
            _typeDependencyEnumerator = typeDependencyEnumerator;
            _traceMessageHandler = traceMessageHandler;
            _configProviderFactory = new ConfigProviderFactory(_traceMessageHandler);
        }

        public ConfiguredDependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _configProviderFactory.SetDefaultInfoImportance(defaultInfoImportance);
            return this;
        }

        public IConfiguredDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = _configProviderFactory.CreateFromXmlConfigFile(configFilePath);
            return new ConfiguredDependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }

        public IConfiguredDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new ConfiguredDependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }
    }
}
