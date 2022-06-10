using System.Xml.Linq;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Extracts config information from an xml config file.
    /// </summary>
    /// <remarks>
    /// Base class handles config load errors so no need to catch exceptions here.
    /// </remarks>
    public sealed class XmlFileConfigProvider : FileConfigProviderBase
    {
        private XDocument _configXDocument;

        public XmlFileConfigProvider(string configFilePath, ConfigFileScope configFileScope, MessageHandler traceMessageHandler)
            : base(configFilePath, configFileScope, traceMessageHandler)
        {
        }

        public override string ToString() => $"XmlConfig:'{ConfigFilePath}'";

        protected override AnalyzerConfigBuilder CreateConfigBuilderFromFile(string configFilePath)
        {
            _configXDocument = XDocument.Load(configFilePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            return XmlConfigParser.Parse(_configXDocument, configFilePath, ConfigFileScope);
        }

        protected override ConfigLoadResult UpdateMaxIssueCountCore(int newValue)
        {
            XmlConfigParser.UpdateMaxIssueCount(_configXDocument, newValue);
            _configXDocument.Save(ConfigFilePath);

            return LoadConfigCore();
        }
    }
}