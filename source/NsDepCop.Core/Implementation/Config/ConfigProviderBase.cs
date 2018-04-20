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
        /// This lock ensures that no property can be read while loading or saving the config.
        /// </summary>
        protected readonly object SaveLoadLockObject = new object();

        protected MessageHandler TraceMessageHandler { get; }
        protected Importance? DefaultInfoImportance { get; private set; }

        protected ConfigProviderBase(MessageHandler traceMessageHandler)
        {
            TraceMessageHandler = traceMessageHandler;
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
                lock (SaveLoadLockObject)
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
                lock (SaveLoadLockObject)
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
                lock (SaveLoadLockObject)
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
                lock (SaveLoadLockObject)
                {
                    EnsureInitialized();
                    return _configLoadResult.ConfigException;
                }
            }
        }

        public void RefreshConfig()
        {
            lock (SaveLoadLockObject)
            {
                EnsureInitialized();
                _configLoadResult = RefreshConfigCore();
            }
        }

        public void UpdateMaxIssueCount(int newValue)
        {
            lock (SaveLoadLockObject)
            {
                EnsureInitialized();

                if (_configLoadResult.ConfigState != AnalyzerConfigState.Enabled)
                    throw new InvalidOperationException($"Cannot {nameof(UpdateMaxIssueCount)} in {_configLoadResult.ConfigState} state.");

                _configLoadResult = UpdateMaxIssueCountCore(newValue);
            }
        }

        protected abstract ConfigLoadResult LoadConfigCore();
        protected abstract ConfigLoadResult RefreshConfigCore();
        protected abstract ConfigLoadResult UpdateMaxIssueCountCore(int newValue);

        protected void EnsureInitialized()
        {
            lock (SaveLoadLockObject)
            {
                if (_isInitialized)
                    return;

                _isInitialized = true;
                _configLoadResult = LoadConfigCore();
            }
        }
    }
}
