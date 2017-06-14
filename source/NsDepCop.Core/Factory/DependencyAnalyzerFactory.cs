using System;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Config;
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
        private Importance? _defaultInfoImportance;

        public DependencyAnalyzerFactory(ITypeDependencyEnumerator typeDependencyEnumerator, MessageHandler traceMessageHandler = null)
        {
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _traceMessageHandler = traceMessageHandler;
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _defaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = new XmlFileConfigProvider(configFilePath, _traceMessageHandler);
            ApplyConfigDefaults(configProvider);
            return new DependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }

        public IDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            var configProvider = new MultiLevelXmlFileConfigProvider(folderPath, _traceMessageHandler);
            ApplyConfigDefaults(configProvider);
            return new DependencyAnalyzer(configProvider, _typeDependencyEnumerator, _traceMessageHandler);
        }

        private void ApplyConfigDefaults(ConfigProviderBase configProvider)
        {
            configProvider.SetDefaultInfoImportance(_defaultInfoImportance);
        }
    }
}
