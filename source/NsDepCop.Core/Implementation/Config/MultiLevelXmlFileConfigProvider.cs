using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Traverses the source tree and reads all config files to create a composite config.
    /// Starts from the specified folder and traverses the folder tree upwards till the root is reached.
    /// </summary>
    /// <remarks>
    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
    /// </remarks>
    internal class MultiLevelXmlFileConfigProvider : ConfigProviderBase
    {
        private readonly string _projectFolder;
        private readonly Parsers? _overridingParser;
        private readonly Action<string> _diagnosticMessageHandler;
        private List<XmlFileConfigProvider> _fileConfigProviders;

        public MultiLevelXmlFileConfigProvider(string projectFolder, Parsers? overridingParser = null, Action<string> diagnosticMessageHandler = null)
        {
            _projectFolder = projectFolder;
            _overridingParser = overridingParser;
            _diagnosticMessageHandler = diagnosticMessageHandler;
            _fileConfigProviders = new List<XmlFileConfigProvider>();

            if (overridingParser != null)
                diagnosticMessageHandler?.Invoke($"Parser overridden with {overridingParser}.");
        }

        protected override AnalyzerState GetState()
        {
            if (!_fileConfigProviders.Any())
                return AnalyzerState.NoConfigFile;

            if (_fileConfigProviders.Any(i => i.ConfigException != null))
                return AnalyzerState.ConfigError;

            if (IsConfigLoaded && !Config.IsEnabled)
                return AnalyzerState.Disabled;

            if (IsConfigLoaded && Config.IsEnabled)
                return AnalyzerState.Enabled;

            throw new Exception("Inconsistent DependencyAnalyzer state.");
        }

        protected override IAnalyzerConfig GetConfig()
        {
            _fileConfigProviders = CreateConfigProviders();

            var configBuilder = new AnalyzerConfigBuilder(_overridingParser);

            foreach (var configProvider in Enumerable.Reverse(_fileConfigProviders))
            {
                _diagnosticMessageHandler?.Invoke($"Found config file: '{configProvider.ConfigFilePath}'");

                if (configProvider.State == AnalyzerState.Enabled)
                    configBuilder.Combine(configProvider.Config);
            }

            return configBuilder.ToAnalyzerConfig();
        }

        private List<XmlFileConfigProvider> CreateConfigProviders()
        {
            return FileFinder.FindInParentFolders(ProductConstants.DefaultConfigFileName, _projectFolder)
                .Select(configFilePath => new XmlFileConfigProvider(configFilePath, _overridingParser, _diagnosticMessageHandler))
                .ToList();
        }
    }
}
