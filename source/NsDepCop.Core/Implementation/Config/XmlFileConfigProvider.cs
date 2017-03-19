using System.Xml.Linq;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Extracts config information from an xml config file.
    /// </summary>
    /// <remarks>
    /// Base class handles config load errors so no need to catch exceptions here.
    /// </remarks>
    internal sealed class XmlFileConfigProvider : FileConfigProviderBase
    {
        public XmlFileConfigProvider(string configFilePath, MessageHandler diagnosticMessageHandler = null)
            : base(configFilePath, diagnosticMessageHandler)
        {
        }

        public override string ToString() => $"XmlConfig:'{ConfigFilePath}'";

        protected override AnalyzerConfigBuilder CreateConfigBuilderFromFile(string configFilePath)
        {
            var configXml = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);
            return XmlConfigParser.Parse(configXml);
        }
    }
}