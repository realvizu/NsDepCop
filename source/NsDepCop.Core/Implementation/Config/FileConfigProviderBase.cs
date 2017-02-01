using System;
using System.IO;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for file based config implementation classes.
    /// Can refresh the config if the file changes.
    /// </summary>
    internal abstract class FileConfigProviderBase : IConfigProvider
    {
        public IProjectConfig Config { get; private set; }

        protected string ConfigFilePath { get; }
        private bool _configFileExists;
        private DateTime _configLastReadUtc;
        private Exception _configException;

        private bool IsConfigLoaded => Config != null;
        private bool IsConfigErroneous => _configException != null;

        public Exception ConfigException => _configException;

        protected FileConfigProviderBase(string configFilePath)
        {
            ConfigFilePath = configFilePath;
            RefreshConfig();
        }

        /// <summary>
        /// Gets the state of the analyzer object.
        /// </summary>
        public ConfigState ConfigState
        {
            get
            {
                if (!_configFileExists)
                    return ConfigState.NoConfigFile;

                if (IsConfigLoaded && !Config.IsEnabled)
                    return ConfigState.Disabled;

                if (IsConfigLoaded && Config.IsEnabled)
                    return ConfigState.Enabled;

                if (!IsConfigLoaded && IsConfigErroneous)
                    return ConfigState.ConfigError;

                throw new Exception("Inconsistent DependencyAnalyzer state.");
            }
        }

        public void RefreshConfig()
        {
            _configFileExists = File.Exists(ConfigFilePath);

            if (!_configFileExists)
            {
                _configException = null;
                return;
            }

            try
            {
                // Read the config if it was never read, or whenever the file changes.
                if (!IsConfigLoaded || ConfigModifiedSinceLastRead())
                {
                    _configLastReadUtc = DateTime.UtcNow;
                    _configException = null;
                    Config = GetConfig();
                }
            }
            catch (Exception e)
            {
                _configException = e;
            }
        }

        protected abstract IProjectConfig GetConfig();

        private bool ConfigModifiedSinceLastRead() => _configLastReadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
    }
}
