using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Traverses the source tree and reads one or multiple config files to create a composite config.
    /// Starts from the specified folder and traverses the folder tree upwards till the root or the max inheritance level is reached.
    /// </summary>
    /// <remarks>
    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
    /// </remarks>
    internal sealed class MultiLevelXmlFileConfigProvider : ConfigProviderBase
    {
        private ConfigLoadResult _lastConfigLoadResult;
        private int _lastInheritanceDepth;

        /// <summary>
        /// The collection of all file config providers that must be composed to a single config.
        /// </summary>
        /// <remarks>
        /// The list is sorted from the project folder's config to increasingly farther configs in the folder tree.
        /// The configs must be combined in reverse order (from the farthest to the closest).
        /// </remarks>
        private List<XmlFileConfigProvider> _fileConfigProviders;

        public string ProjectFolder { get; }

        public MultiLevelXmlFileConfigProvider(string projectFolder, Action<string> diagnosticMessageHandler = null)
            : base(diagnosticMessageHandler)
        {
            ProjectFolder = projectFolder;
        }

        public int InheritanceDepth
        {
            get
            {
                lock (RefreshLockObject)
                {
                    EnsureInitialized();
                    return _fileConfigProviders.First().InheritanceDepth;
                }
            }
        }

        public override string ToString() => $"MultiLevelXmlConfig:'{ProjectFolder}'";

        protected override ConfigLoadResult LoadConfigCore()
        {
            var projectLevelConfigProvider = new XmlFileConfigProvider(GetConfigFilePath(ProjectFolder), DiagnosticMessageHandler);

            _fileConfigProviders = CreateFileConfigProviderList(projectLevelConfigProvider, ProjectFolder);

            return CombineFileConfigProvidersAndSaveResult();
        }

        protected override ConfigLoadResult RefreshConfigCore()
        {
            if (!AnyChildConfigChanged())
                return _lastConfigLoadResult;

            DiagnosticMessageHandler?.Invoke($"Refreshing config {this}.");

            var projectLevelConfigProvider = _fileConfigProviders[0];
            projectLevelConfigProvider.RefreshConfig();

            if (InheritanceDepth != _lastInheritanceDepth)
            {
                _fileConfigProviders = CreateFileConfigProviderList(projectLevelConfigProvider, ProjectFolder);
            }
            else
            {
                foreach (var configProvider in _fileConfigProviders.Skip(1))
                    configProvider.RefreshConfig();
            }

            return CombineFileConfigProvidersAndSaveResult();
        }

        private ConfigLoadResult CombineFileConfigProvidersAndSaveResult()
        {
            _lastInheritanceDepth = InheritanceDepth;
            _lastConfigLoadResult = CombineFileConfigProviders();
            return _lastConfigLoadResult;
        }

        private ConfigLoadResult CombineFileConfigProviders()
        {
            var configBuilder = CreateAnalyzerConfigBuilder();

            var anyConfigFound = false;
            foreach (var childConfigProvider in Enumerable.Reverse(_fileConfigProviders))
            {
                var childConfigState = childConfigProvider.ConfigState;
                switch (childConfigState)
                {
                    case AnalyzerConfigState.NoConfig:
                        break;

                    case AnalyzerConfigState.Disabled:
                        anyConfigFound = true;
                        configBuilder.SetIsEnabled(false);
                        break;

                    case AnalyzerConfigState.Enabled:
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
                ? ConfigLoadResult.CreateWithConfig(configBuilder)
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

        private List<XmlFileConfigProvider> CreateFileConfigProviderList(XmlFileConfigProvider firstConfigProvider, string startFolderPath)
        {
            var fileConfigProviders = new List<XmlFileConfigProvider> { firstConfigProvider };

            DiagnosticMessageHandler?.Invoke($"InheritanceDepth={firstConfigProvider.InheritanceDepth}");

            var currentFolder = startFolderPath;
            for (var i = 0; i < firstConfigProvider.InheritanceDepth; i++)
            {
                currentFolder = Directory.GetParent(currentFolder)?.FullName;
                if (string.IsNullOrWhiteSpace(currentFolder))
                    break;

                var higherLevelConfigProvider = new XmlFileConfigProvider(GetConfigFilePath(currentFolder), DiagnosticMessageHandler);
                fileConfigProviders.Add(higherLevelConfigProvider);
            }

            return fileConfigProviders;
        }

        private static string GetConfigFilePath(string folderPath) => Path.Combine(folderPath, ProductConstants.DefaultConfigFileName);
    }
}
