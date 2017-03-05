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
        private readonly Parsers? _overridingParser;

        public XmlFileConfigProvider(string configFilePath, Parsers? overridingParser = null, Action<string> diagnosticMessageHandler = null)
            : base(configFilePath, diagnosticMessageHandler)
        {
            _overridingParser = overridingParser;

            if (overridingParser != null)
                diagnosticMessageHandler?.Invoke($"Parser overridden with {overridingParser}.");
        }

        public override string ToString() => $"XmlFileConfigProvider({ConfigFilePath})";

        protected override IAnalyzerConfig LoadConfigFromFile(string configFilePath)
        {
            try
            {
                var configXml = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);
                var config = XmlConfigParser.Parse(configXml);

                return new AnalyzerConfigBuilder(_overridingParser).Combine(config).ToAnalyzerConfig();
            }
            catch (Exception e)
            {
                throw new Exception($"Error in '{configFilePath}': {e.Message}", e);
            }
        }
    }
}