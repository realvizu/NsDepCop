using System;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public class DependencyAnalyzerFactory : IDependencyAnalyzerFactory, IConfigInitializer<DependencyAnalyzerFactory>
    {
        private readonly Action<string> _diagnosticMessageHandler;
        private Parsers? _overridingParser;
        private Parsers? _defaultParser;
        private Importance? _defaultInfoImportance;

        public DependencyAnalyzerFactory(Action<string> diagnosticMessageHandler = null)
        {
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public DependencyAnalyzerFactory OverrideParser(Parsers? overridingParser)
        {
            _overridingParser = overridingParser;
            return this;
        }

        public DependencyAnalyzerFactory SetDefaultParser(Parsers? defaultParser)
        {
            _defaultParser = defaultParser;
            return this;
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _defaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath)
        {
            var configProvider = CreateConfigProvider(configFilePath);
            return new DependencyAnalyzer(configProvider, _diagnosticMessageHandler);
        }

        private XmlFileConfigProvider CreateConfigProvider(string configFilePath)
        {
            return new XmlFileConfigProvider(configFilePath, _diagnosticMessageHandler)
                .OverrideParser(_overridingParser)
                .SetDefaultParser(_defaultParser)
                .SetDefaultInfoImportance(_defaultInfoImportance)
                as XmlFileConfigProvider;
        }
    }
}
