using System;
using System.IO;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for file based config implementations.
    /// </summary>
    internal abstract class FileConfigProviderBase : IConfigProvider
    {
        protected readonly string ConfigFilePath;
        private readonly object _isInitializedLock = new object();
        private bool _isInitialized;

        private bool _configFileExists;
        private DateTime _configLastLoadUtc;
        private IAnalyzerConfig _config;
        private Exception _configException;

        protected FileConfigProviderBase(string configFilePath)
        {
            ConfigFilePath = configFilePath;
        }

        private bool IsConfigLoaded => _config != null;
        private bool IsConfigErroneous => _configException != null;

        public IAnalyzerConfig Config
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _config;
                }
            }
        }

        public AnalyzerState State
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return GetState();
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _configException;
                }
            }
        }

        public void RefreshConfig()
        {
            _configFileExists = File.Exists(ConfigFilePath);

            if (!_configFileExists)
            {
                _configException = null;
                _config = null;
                return;
            }

            try
            {
                if (!IsConfigLoaded || ConfigModifiedSinceLastLoad())
                {
                    _configLastLoadUtc = DateTime.UtcNow;
                    _configException = null;
                    _config = LoadConfig(ConfigFilePath);
                }
            }
            catch (Exception e)
            {
                _configException = e;
            }
        }

        protected abstract IAnalyzerConfig LoadConfig(string configFilePath);

        private void Initialize()
        {
            _isInitialized = true;
            RefreshConfig();
        }

        private AnalyzerState GetState()
        {
            if (!_configFileExists)
                return AnalyzerState.NoConfigFile;

            if (IsConfigLoaded && !Config.IsEnabled)
                return AnalyzerState.Disabled;

            if (IsConfigLoaded && Config.IsEnabled)
                return AnalyzerState.Enabled;

            if (!IsConfigLoaded && IsConfigErroneous)
                return AnalyzerState.ConfigError;

            throw new Exception("Inconsistent DependencyAnalyzer state.");
        }

        private bool ConfigModifiedSinceLastLoad()
        {
            return _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }
    }
}
