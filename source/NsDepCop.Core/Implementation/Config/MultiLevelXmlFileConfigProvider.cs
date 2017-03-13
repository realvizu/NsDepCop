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
    internal sealed class MultiLevelXmlFileConfigProvider : ConfigProviderBase
    {
        /// <summary>
        /// Just a precaution to avoid runaway folder traversals.
        /// </summary>
        private const int MaxFolderLevelsToTraverse = 10;

        private ConfigLoadResult _lastConfigLoadResult;

        public string ProjectFolder { get; }

        /// <summary>
        /// The collection of all file config providers that must be composed to a single config.
        /// Already sorted from most general (closer to root folder) to most specific (the project folder).
        /// </summary>
        private readonly List<XmlFileConfigProvider> _fileConfigProviders;

        public MultiLevelXmlFileConfigProvider(string projectFolder, Action<string> diagnosticMessageHandler = null)
            : base(diagnosticMessageHandler)
        {
            ProjectFolder = projectFolder;
            _fileConfigProviders = CreateConfigProviders();
        }

        public override string ToString() => $"MultiLevelXmlConfig:'{ProjectFolder}'";

        protected override ConfigLoadResult LoadConfigCore()
        {
            _lastConfigLoadResult = CombineFileConfigProviders();
            return _lastConfigLoadResult;
        }

        protected override ConfigLoadResult RefreshConfigCore()
        {
            if (!AnyChildConfigChanged())
                return _lastConfigLoadResult;

            DiagnosticMessageHandler?.Invoke($"Refreshing config {this}.");

            foreach (var configProvider in _fileConfigProviders)
                configProvider.RefreshConfig();

            return LoadConfigCore();
        }

        private ConfigLoadResult CombineFileConfigProviders()
        {
            var configBuilder = CreateAnalyzerConfigBuilder();

            var anyConfigFound = false;
            foreach (var childConfigProvider in _fileConfigProviders)
            {
                var childConfigState = childConfigProvider.ConfigState;
                switch (childConfigState)
                {
                    case AnalyzerConfigState.NoConfig:
                        break;

                    case AnalyzerConfigState.Enabled:
                    case AnalyzerConfigState.Disabled:
                        anyConfigFound = true;
                        configBuilder.Combine(childConfigProvider.ConfigBuilder);
                        break;

                    case AnalyzerConfigState.ConfigError:
                        return ConfigLoadResult.CreateWithError(childConfigProvider.ConfigException);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(childConfigState), childConfigState, null);
                }
            }

            return anyConfigFound
                ? ConfigLoadResult.CreateWithConfig(configBuilder.ToAnalyzerConfig())
                : ConfigLoadResult.CreateWithNoConfig();
        }

        private AnalyzerConfigBuilder CreateAnalyzerConfigBuilder()
        {
            return new AnalyzerConfigBuilder()
                .OverrideParser(OverridingParser)
                .SetDefaultParser(DefaultParser)
                .SetDefaultInfoImportance(DefaultInfoImportance);
        }

        private bool AnyChildConfigChanged() => _fileConfigProviders.Any(i => i.HasConfigFileChanged());

        private List<XmlFileConfigProvider> CreateConfigProviders()
        {
            return FileHelper.GetFilenameWithFolderPaths(ProductConstants.DefaultConfigFileName, ProjectFolder, MaxFolderLevelsToTraverse)
                .OrderBy(i => i.Length)
                .Select(i => new XmlFileConfigProvider(i, DiagnosticMessageHandler))
                .ToList();
        }
    }
}
