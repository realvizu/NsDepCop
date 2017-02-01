using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer instances.
    /// </summary>
    public static class DependencyAnalyzerFactory
    {
        public static IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath, Parsers? overridingParser = null)
        {
            var configProvider = new XmlFileConfigProvider(configFilePath);
            return new DependencyAnalyzer(configProvider, overridingParser);
        }
    }
}
