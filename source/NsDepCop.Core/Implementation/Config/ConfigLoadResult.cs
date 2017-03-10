using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Describes the result of a config load operation.
    /// </summary>
    internal struct ConfigLoadResult
    {
        public AnalyzerConfigState ConfigState { get; }
        public IAnalyzerConfig Config { get; }
        public Exception ConfigException { get; }

        private ConfigLoadResult(AnalyzerConfigState configState, IAnalyzerConfig config, Exception configException)
        {
            ConfigException = configException;
            Config = config;
            ConfigState = configState;
        }

        public static ConfigLoadResult CreateWithError(Exception configException)
        {
            if (configException == null)
                throw new ArgumentNullException(nameof(configException));

            return new ConfigLoadResult(AnalyzerConfigState.ConfigError, null, configException);
        }

        public static ConfigLoadResult CreateWithNoConfig()
        {
            return new ConfigLoadResult(AnalyzerConfigState.NoConfig, null, null);
        }

        public static ConfigLoadResult CreateWithConfig(IAnalyzerConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (config.IsEnabled)
                return new ConfigLoadResult(AnalyzerConfigState.Enabled, config, null);
            else
                return new ConfigLoadResult(AnalyzerConfigState.Disabled, null, null);
        }
    }
}
