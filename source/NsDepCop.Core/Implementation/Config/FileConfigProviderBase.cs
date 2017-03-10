using System;
using System.Diagnostics;
using System.IO;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for file based config implementations.
    /// Reloads only if the file changed.
    /// </summary>
    /// <remarks>
    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
    /// </remarks>
    internal abstract class FileConfigProviderBase : ConfigProviderBase
    {
        private bool _configFileExists;
        private DateTime _configLastLoadUtc;
        private ConfigLoadResult _lastConfigLoadResult;

        public string ConfigFilePath { get; }
        public AnalyzerConfigBuilder ConfigBuilder { get; private set; }

        protected FileConfigProviderBase(string configFilePath, Parsers? overridingParser, Action<string> diagnosticMessageHandler)
            : base(overridingParser, diagnosticMessageHandler)
        {
            ConfigFilePath = configFilePath;
        }

        public bool HasConfigFileChanged()
        {
            return ConfigFileCreatedOrDeleted() 
                || ConfigFileModifiedSinceLastLoad();
        }

        protected override ConfigLoadResult LoadConfigCore()
        {
            _lastConfigLoadResult = LoadConfigFromFile();
            return _lastConfigLoadResult;
        }

        protected override ConfigLoadResult RefreshConfigCore()
        {
            if (! HasConfigFileChanged())
                return  _lastConfigLoadResult;

            DiagnosticMessageHandler?.Invoke($"Refreshing config {this}.");
            return LoadConfigCore();
        }

        private ConfigLoadResult LoadConfigFromFile()
        {
            try
            {
                _configFileExists = File.Exists(ConfigFilePath);
                if (!_configFileExists)
                {
                    DiagnosticMessageHandler?.Invoke($"Config file '{ConfigFilePath}' not found.");
                    return ConfigLoadResult.CreateWithNoConfig();
                }

                _configLastLoadUtc = DateTime.UtcNow;

                ConfigBuilder = CreateConfigBuilderFromFile(ConfigFilePath);

                Debug.Assert(ConfigBuilder != null);
                var config = ConfigBuilder.ToAnalyzerConfig();

                return ConfigLoadResult.CreateWithConfig(config);
            }
            catch (Exception e)
            {
                DiagnosticMessageHandler?.Invoke($"BuildConfig exception: {e}");
                return ConfigLoadResult.CreateWithError(e);
            }
        }

        protected abstract AnalyzerConfigBuilder CreateConfigBuilderFromFile(string configFilePath);

        private bool ConfigFileCreatedOrDeleted()
        {
            return _configFileExists != File.Exists(ConfigFilePath);
        }

        private bool ConfigFileModifiedSinceLastLoad()
        {
            return File.Exists(ConfigFilePath) 
                && _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }
    }
}
