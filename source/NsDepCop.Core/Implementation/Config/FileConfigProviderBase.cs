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

        protected FileConfigProviderBase(string configFilePath, Action<string> diagnosticMessageHandler = null)
            : base(diagnosticMessageHandler)
        {
            ConfigFilePath = configFilePath;
        }

        protected override IAnalyzerConfig GetConfig()
        {
            _configFileExists = File.Exists(ConfigFilePath);

            if (!_configFileExists)
            {
                DiagnosticMessageHandler?.Invoke($"Config file '{ConfigFilePath}' not found.");
                return null;
            }

            if (IsConfigLoaded && !ConfigModifiedSinceLastLoad())
                return Config;

            if (!IsConfigLoaded)
                DiagnosticMessageHandler?.Invoke($"Loading config file '{ConfigFilePath}' for the first time.");
            else
                DiagnosticMessageHandler?.Invoke($"Reloading modified config file '{ConfigFilePath}'.");

            _configLastLoadUtc = DateTime.UtcNow;
            return LoadConfigFromFile(ConfigFilePath);
        }

        protected abstract IAnalyzerConfig LoadConfigFromFile(string configFilePath);

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

        private bool ConfigModifiedSinceLastLoad()
        {
            return _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }
    }
}
