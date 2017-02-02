using System;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Extracts config information from xml config files.
    /// </summary>
    internal sealed class XmlFileConfigProvider : FileConfigProviderBase
    {
        private readonly Parsers? _overridingParser;

        public XmlFileConfigProvider(string configFilePath, Parsers? overridingParser = null)
            : base(configFilePath)
        {
            _overridingParser = overridingParser;
        }

        protected override IProjectConfig LoadConfig(string configFilePath)
        {
            try
            {
                var configXml = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);
                var config = XmlConfigParser.ParseXmlConfig(configXml);

                return new ProjectConfigBuilder(_overridingParser).Combine(config).ToProjectConfig();
            }
            catch (Exception e)
            {
                throw new Exception($"Error in '{configFilePath}': {e.Message}", e);
            }
        }
    }
}