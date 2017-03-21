using System;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for config provider implementations.
    /// </summary>
    /// <remarks>
    /// Uses locking to ensure that no property can be read while refreshing the config.
    /// </remarks>
    internal abstract class ConfigProviderBase : IConfigProvider, IConfigInitializer<ConfigProviderBase>
    {
        private bool _isInitialized;
        private ConfigLoadResult _configLoadResult;

        /// <summary>
        /// This lock ensures that no property can be read while refreshing the config.
        /// </summary>
        protected readonly object RefreshLockObject = new object();

        protected MessageHandler InfoMessageHandler { get; }
        protected MessageHandler DiagnosticMessageHandler { get; }
        protected Importance? DefaultInfoImportance { get; private set; }

        protected ConfigProviderBase(MessageHandler infoMessageHandler, MessageHandler diagnosticMessageHandler)
        {
            InfoMessageHandler = infoMessageHandler;
            DiagnosticMessageHandler = diagnosticMessageHandler;
        }

        public ConfigProviderBase SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            DefaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public IAnalyzerConfig Config
        {
            get
            {
                lock (RefreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.Config;
                }
            }
        }

        public AnalyzerConfigBuilder ConfigBuilder
        {
            get
            {
                lock (RefreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigBuilder;
                }
            }
        }

        public AnalyzerConfigState ConfigState
        {
            get
            {
                lock (RefreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigState;
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (RefreshLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigException;
                }
            }
        }

        public void RefreshConfig()
        {
            lock (RefreshLockObject)
            {
                EnsureInitialized();
                _configLoadResult = RefreshConfigCore();
            }
        }

        protected abstract ConfigLoadResult LoadConfigCore();
        protected abstract ConfigLoadResult RefreshConfigCore();

        protected void EnsureInitialized()
        {
                if (_isInitialized)
                    return;

                _isInitialized = true;
                _configLoadResult = LoadConfigCore();
        }
    }
}
