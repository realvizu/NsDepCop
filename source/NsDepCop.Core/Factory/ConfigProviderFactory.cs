using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates config provider objects.
    /// </summary>
    public class ConfigProviderFactory : IConfigProviderFactory
    {
        private readonly MessageHandler _traceMessageHandler;
        private Importance? _defaultInfoImportance;

        public ConfigProviderFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
        }

        public ConfigProviderFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _defaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IConfigProvider CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = new XmlFileConfigProvider(configFilePath, _traceMessageHandler);
            ApplyConfigDefaults(configProvider);
            return configProvider;
        }

        public IConfigProvider CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            var configProvider = new MultiLevelXmlFileConfigProvider(folderPath, _traceMessageHandler);
            ApplyConfigDefaults(configProvider);
            return configProvider;
        }

        private void ApplyConfigDefaults(ConfigProviderBase configProvider)
        {
            configProvider.SetDefaultInfoImportance(_defaultInfoImportance);
        }
    }
}
