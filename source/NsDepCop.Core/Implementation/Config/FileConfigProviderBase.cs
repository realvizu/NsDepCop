using System;
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

        public string ConfigFilePath { get; }

        protected FileConfigProviderBase(string configFilePath, Parsers? overridingParser, Action<string> diagnosticMessageHandler)
            : base(overridingParser, diagnosticMessageHandler)
        {
            ConfigFilePath = configFilePath;
        }

        protected override AnalyzerState GetState()
        {
            if (!_configFileExists)
                return AnalyzerState.NoConfigFile;

            if (IsConfigErroneous)
                return AnalyzerState.ConfigError;

            if (IsConfigLoaded && !Config.IsEnabled)
                return AnalyzerState.Disabled;

            if (IsConfigLoaded && Config.IsEnabled)
                return AnalyzerState.Enabled;

            throw new Exception("Inconsistent analyzer state.");
        }

        protected override AnalyzerConfigBuilder BuildConfig()
        {
            _configFileExists = File.Exists(ConfigFilePath);

            if (!_configFileExists)
            {
                DiagnosticMessageHandler?.Invoke($"Config file '{ConfigFilePath}' not found.");
                return null;
            }

            if (IsConfigLoaded && !ConfigModifiedSinceLastLoad())
                return ConfigBuilder;

            if (!IsConfigLoaded)
                DiagnosticMessageHandler?.Invoke($"Loading config file '{ConfigFilePath}' for the first time.");
            else
                DiagnosticMessageHandler?.Invoke($"Reloading modified config file '{ConfigFilePath}'.");

            _configLastLoadUtc = DateTime.UtcNow;
             return LoadConfigFromFile(ConfigFilePath);
        }

        protected abstract AnalyzerConfigBuilder LoadConfigFromFile(string configFilePath);

        private bool ConfigModifiedSinceLastLoad()
        {
            return _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }
    }
}
