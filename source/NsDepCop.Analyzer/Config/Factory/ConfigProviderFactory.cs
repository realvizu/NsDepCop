using Codartis.NsDepCop.Config.Implementation;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config.Factory
{
    /// <summary>
    /// Creates config provider objects.
    /// </summary>
    public class ConfigProviderFactory : IConfigProviderFactory
    {
        private readonly MessageHandler _traceMessageHandler;

        public ConfigProviderFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
        }

        public IUpdateableConfigProvider CreateFromXmlConfigFile(string configFilePath)
        {
            return new XmlFileConfigProvider(configFilePath, ConfigFileScope.SingleCompilation, _traceMessageHandler);
        }

        public IUpdateableConfigProvider CreateFromMultiLevelXmlConfigFile(string folderPath)
        {
            return new MultiLevelXmlFileConfigProvider(folderPath, _traceMessageHandler);
        }
    }
}