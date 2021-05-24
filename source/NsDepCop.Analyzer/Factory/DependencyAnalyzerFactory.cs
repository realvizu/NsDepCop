using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public sealed class DependencyAnalyzerFactory : IDependencyAnalyzerFactory, IConfigInitializer<DependencyAnalyzerFactory>
    {
        private readonly MessageHandler _traceMessageHandler;
        private readonly IConfigProviderFactory _configProviderFactory;

        public DependencyAnalyzerFactory(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;
            _configProviderFactory = new ConfigProviderFactory(_traceMessageHandler);
        }

        public DependencyAnalyzerFactory SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            _configProviderFactory.SetDefaultInfoImportance(defaultInfoImportance);
            return this;
        }

        public IDependencyAnalyzer Create(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(folderPath);
            return new DependencyAnalyzer(configProvider, typeDependencyEnumerator, _traceMessageHandler);
        }
    }
}
