using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates analyzer objects that manage their own config.
    /// </summary>
    public class ConfiguredAnalyzerFactory
    {
        public IConfiguredAnalyzer CreateFromXmlConfigFile(string configFilePath, Parsers? overridingParser = null)
        {
            var configProvider = new ConfigProviderFactory().CreateFromXmlConfigFile(configFilePath);
            return new ConfiguredAnalyzer(configProvider, new AnalyzerFactory(), overridingParser);
        }
    }
}
