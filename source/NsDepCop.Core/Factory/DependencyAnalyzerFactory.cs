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
        private readonly MessageHandler _infoMessageHandler;
        private readonly MessageHandler _diagnosticMessageHandler;
        private Importance? _defaultInfoImportance;

        public DependencyAnalyzerFactory(ITypeDependencyEnumerator typeDependencyEnumerator,
            MessageHandler infoMessageHandler = null, MessageHandler diagnosticMessageHandler = null)
        {
            if (typeDependencyEnumerator == null)
                throw new ArgumentNullException(nameof(typeDependencyEnumerator));

            _typeDependencyEnumerator = typeDependencyEnumerator;
            _infoMessageHandler = infoMessageHandler;
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _defaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = new XmlFileConfigProvider(configFilePath, _infoMessageHandler, _diagnosticMessageHandler);
            ApplyConfigDefaults(configProvider);
            return new DependencyAnalyzer(configProvider, _typeDependencyEnumerator);
        }

        public IDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            var configProvider = new MultiLevelXmlFileConfigProvider(folderPath, _infoMessageHandler, _diagnosticMessageHandler);
            ApplyConfigDefaults(configProvider);
            return new DependencyAnalyzer(configProvider, _typeDependencyEnumerator);
        }

        private void ApplyConfigDefaults(ConfigProviderBase configProvider)
        {
            configProvider.SetDefaultInfoImportance(_defaultInfoImportance);
        }
    }
}
