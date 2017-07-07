using System;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public class DependencyAnalyzerFactory : IDependencyAnalyzerFactory, IConfigInitializer<DependencyAnalyzerFactory>
    {
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly MessageHandler _traceMessageHandler;
        private readonly IConfigProviderFactory _configProviderFactory;

        public DependencyAnalyzerFactory(ITypeDependencyEnumerator typeDependencyEnumerator, MessageHandler traceMessageHandler)
        {
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _traceMessageHandler = traceMessageHandler;
            _configProviderFactory = new ConfigProviderFactory(_traceMessageHandler);
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _configProviderFactory.SetDefaultInfoImportance(defaultInfoImportance);
            return this;
        }

        public IDependencyAnalyzer Create(IAnalyzerConfig config)
        {
            return new DependencyAnalyzer(config, _typeDependencyEnumerator, _traceMessageHandler);
        }

        public IRefreshableDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = _configProviderFactory.CreateFromXmlConfigFile(configFilePath);
            return new RefreshableDependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }

        public IRefreshableDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new RefreshableDependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }
    }
}
