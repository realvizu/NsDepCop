using System;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Extracts config information from an xml config file.
    /// </summary>
    internal sealed class XmlFileConfigProvider : FileConfigProviderBase
    {
        public XmlFileConfigProvider(string configFilePath, Parsers? overridingParser = null, Action<string> diagnosticMessageHandler = null)
            : base(configFilePath, overridingParser, diagnosticMessageHandler)
        {
        }

        public override string ToString() => $"XmlConfig:'{ConfigFilePath}'";

        protected override AnalyzerConfigBuilder LoadConfigFromFile(string configFilePath)
        {
            try
            {
                var configXml = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);
                return XmlConfigParser.Parse(configXml, OverridingParser);
            }
            catch (Exception e)
            {
                throw new Exception($"Error in '{configFilePath}': {e.Message}", e);
            }
        }
    }
}