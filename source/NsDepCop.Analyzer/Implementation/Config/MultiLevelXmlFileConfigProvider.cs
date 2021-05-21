using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Interface;
using Codartis.NsDepCop.Interface.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Implementation.Config
{
    /// <summary>
    /// Traverses the source tree and reads one or multiple config files to create a composite config.
    /// Starts from the specified folder and traverses the folder tree upwards till the root or the max inheritance level is reached.
    /// </summary>
    /// <remarks>
    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
    /// </remarks>
    public sealed class MultiLevelXmlFileConfigProvider : ConfigProviderBase
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

        public MultiLevelXmlFileConfigProvider(string projectFolder, MessageHandler traceMessageHandler)
            : base(traceMessageHandler)
        {
            ProjectFolder = projectFolder;
        }

        public override string ConfigLocation => ProjectFolder;

        public int InheritanceDepth
        {
            get
            {
                lock (SaveLoadLockObject)
                {
                    EnsureInitialized();
                    return _fileConfigProviders.First().InheritanceDepth;
                }
            }
        }

        public override string ToString() => $"MultiLevelXmlConfig:'{ProjectFolder}'";

        protected override ConfigLoadResult LoadConfigCore()
        {
            LogTraceMessage($"Loading config {this}");

            var projectLevelConfigProvider = new XmlFileConfigProvider(GetConfigFilePath(ProjectFolder), TraceMessageHandler);

            _fileConfigProviders = CreateFileConfigProviderList(projectLevelConfigProvider, ProjectFolder);

            return CombineFileConfigProvidersAndSaveResult();
        }

        protected override ConfigLoadResult RefreshConfigCore()
        {
            if (!AnyChildConfigChanged())
                return _lastConfigLoadResult;

            LogTraceMessage($"Refreshing config {this}.");

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

        protected override ConfigLoadResult UpdateMaxIssueCountCore(int newValue)
        {
            _fileConfigProviders.First().UpdateMaxIssueCount(newValue);

            return CombineFileConfigProvidersAndSaveResult();
        }

        private ConfigLoadResult CombineFileConfigProvidersAndSaveResult()
        {
            _lastInheritanceDepth = InheritanceDepth;
            _lastConfigLoadResult = CombineFileConfigProviders();

            LogTraceMessage(IndentHelper.Indent("Effective config:", 1).Concat(IndentHelper.Indent(_lastConfigLoadResult.ToStrings(), 2)));

            return _lastConfigLoadResult;
        }

        private ConfigLoadResult CombineFileConfigProviders()
        {
            var configBuilder = CreateAnalyzerConfigBuilder();

            var anyConfigFound = false;
            foreach (var childConfigProvider in Enumerable.Reverse(_fileConfigProviders))
            {
                var childConfigState = childConfigProvider.ConfigState;

                LogTraceMessage(IndentHelper.Indent($"Combining {childConfigProvider}, state={childConfigState}", 1));

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

                        var childConfigBuilder = childConfigProvider.ConfigBuilder;
                        configBuilder.Combine(childConfigBuilder);
                        LogTraceMessage(IndentHelper.Indent(childConfigBuilder.ToStrings(), 2));
                        break;

                    case AnalyzerConfigState.ConfigError:
                        return ConfigLoadResult.CreateWithError(childConfigProvider.ConfigException);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(childConfigState), childConfigState, "Unexpected value.");
                }
            }

            return anyConfigFound
                ? ConfigLoadResult.CreateWithConfig(configBuilder)
                : ConfigLoadResult.CreateWithNoConfig();
        }

        private AnalyzerConfigBuilder CreateAnalyzerConfigBuilder()
        {
            if (DefaultInfoImportance.HasValue)
                LogTraceMessage(IndentHelper.Indent($"DefaultInfoImportance={DefaultInfoImportance}", 1));

            return new AnalyzerConfigBuilder().SetDefaultInfoImportance(DefaultInfoImportance);
        }

        private bool AnyChildConfigChanged() => _fileConfigProviders.Any(i => i.HasConfigFileChanged());

        private List<XmlFileConfigProvider> CreateFileConfigProviderList(XmlFileConfigProvider firstConfigProvider, string startFolderPath)
        {
            var fileConfigProviders = new List<XmlFileConfigProvider> { firstConfigProvider };

            LogTraceMessage(IndentHelper.Indent($"InheritanceDepth={firstConfigProvider.InheritanceDepth}", 1));

            var currentFolder = startFolderPath;
            for (var i = 0; i < firstConfigProvider.InheritanceDepth; i++)
            {
                currentFolder = Directory.GetParent(currentFolder)?.FullName;
                if (string.IsNullOrWhiteSpace(currentFolder))
                    break;

                var higherLevelConfigProvider = new XmlFileConfigProvider(GetConfigFilePath(currentFolder), TraceMessageHandler);
                fileConfigProviders.Add(higherLevelConfigProvider);
            }

            return fileConfigProviders;
        }

        private static string GetConfigFilePath(string folderPath) => Path.Combine(folderPath, ProductConstants.DefaultConfigFileName);

        private void LogTraceMessage(IEnumerable<string> messages)
        {
            foreach (var message in messages)
                LogTraceMessage(message);
        }

        private void LogTraceMessage(string message) => TraceMessageHandler?.Invoke(message);
    }
}
