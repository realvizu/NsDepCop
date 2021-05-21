using System.Xml.Linq;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Implementation.Config
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

        public XmlFileConfigProvider(string configFilePath, MessageHandler traceMessageHandler)
            : base(configFilePath, traceMessageHandler)
        {
        }

        public override string ToString() => $"XmlConfig:'{ConfigFilePath}'";

        protected override AnalyzerConfigBuilder CreateConfigBuilderFromFile(string configFilePath)
        {
            _configXDocument = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);
            return XmlConfigParser.Parse(_configXDocument);
        }

        protected override ConfigLoadResult UpdateMaxIssueCountCore(int newValue)
        {
            XmlConfigParser.UpdateMaxIssueCount(_configXDocument, newValue);
            _configXDocument.Save(ConfigFilePath);

            return LoadConfigCore();
        }
    }
}