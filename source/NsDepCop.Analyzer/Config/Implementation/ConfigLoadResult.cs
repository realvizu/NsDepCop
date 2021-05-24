using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Describes the result of a config load operation.
    /// </summary>
    public readonly struct ConfigLoadResult 
    {
        public AnalyzerConfigState ConfigState { get; }
        public AnalyzerConfigBuilder ConfigBuilder { get; }
        public IAnalyzerConfig Config { get; }
        public Exception ConfigException { get; }

        private ConfigLoadResult(AnalyzerConfigState configState, AnalyzerConfigBuilder configBuilder, 
           IAnalyzerConfig config, Exception configException)
        {
            ConfigState = configState;
            ConfigBuilder = configBuilder;
            Config = config;
            ConfigException = configException;
        }

        public static ConfigLoadResult CreateWithError(Exception configException)
        {
            if (configException == null)
                throw new ArgumentNullException(nameof(configException));

            return new ConfigLoadResult(AnalyzerConfigState.ConfigError, null, null, configException);
        }

        public static ConfigLoadResult CreateWithNoConfig()
        {
            return new ConfigLoadResult(AnalyzerConfigState.NoConfig, null, null, null);
        }

        public static ConfigLoadResult CreateWithConfig(AnalyzerConfigBuilder configBuilder)
        {
            if (configBuilder == null)
                throw new ArgumentNullException(nameof(configBuilder));

            var config = configBuilder.ToAnalyzerConfig();
            return config.IsEnabled 
                ? new ConfigLoadResult(AnalyzerConfigState.Enabled, configBuilder, config, null) 
                : new ConfigLoadResult(AnalyzerConfigState.Disabled, null, null, null);
        }

        public IEnumerable<string> ToStrings()
        {
            yield return $"ConfigState={ConfigState}";
            if (ConfigException != null) yield return $"ConfigException={ConfigException}";
            if (Config != null) foreach (var s in Config.ToStrings()) yield return s;
        }
    }
}
