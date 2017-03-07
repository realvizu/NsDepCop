using System;
using Codartis.NsDepCop.Core.Interface.Config;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for config provider implementations.
    /// Loads the config when properties are first accessed.
    /// </summary>
    /// <remarks>
    /// Uses locking to make every operation atomic (including property reads).
    /// </remarks>
    internal abstract class ConfigProviderBase : IConfigProvider
    {
        protected Parsers? OverridingParser { get; }
        protected Action<string> DiagnosticMessageHandler { get; }

        private readonly object _lockObject = new object();
        private bool _isInitialized;

        private AnalyzerConfigBuilder _configBuilder;
        private Exception _configException;
        private IAnalyzerConfig _config;
        private AnalyzerState _state;

        protected ConfigProviderBase(Parsers? overridingParser, Action<string> diagnosticMessageHandler)
        {
            OverridingParser = overridingParser;
            DiagnosticMessageHandler = diagnosticMessageHandler;

            if (overridingParser != null)
                diagnosticMessageHandler?.Invoke($"Parser overridden with {overridingParser}.");
        }

        protected bool IsConfigLoaded => _config != null;
        protected bool IsConfigErroneous => _configException != null;

        public AnalyzerConfigBuilder ConfigBuilder
        {
            get
            {
                lock (_lockObject)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _configBuilder;
                }
            }
        }

        public IAnalyzerConfig Config
        {
            get
            {
                lock (_lockObject)
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
                lock (_lockObject)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _state;
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (_lockObject)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _configException;
                }
            }
        }

        public void RefreshConfig()
        {
            lock (_lockObject)
            {
                if (IsRefreshNeeded())
                {
                    DiagnosticMessageHandler?.Invoke($"Reloading config {this}.");
                    BuildConfig();
                }
            }
        }

        protected abstract AnalyzerState GetState();
        protected abstract AnalyzerConfigBuilder GetConfigBuilder();
        protected virtual bool IsRefreshNeeded() => true;

        private void Initialize()
        {
            _isInitialized = true;
            DiagnosticMessageHandler?.Invoke($"Loading config {this}.");
            BuildConfig();
        }

        private void BuildConfig()
        {
            try
            {
                _configBuilder = GetConfigBuilder();

                _configException = null;
                _config = ConfigBuilder?.ToAnalyzerConfig();
                _state = GetState();

                DumpConfigToDiagnosticOutput();
            }
            catch (Exception e)
            {
                _configException = e;
                _config = null;
                _state = AnalyzerState.ConfigError;

                DiagnosticMessageHandler?.Invoke($"BuildConfig exception: {e}");
            }
            finally
            {
                DiagnosticMessageHandler?.Invoke($"Config state={_state}");
            }
        }

        private void DumpConfigToDiagnosticOutput()
        {
            _config?.DumpToStrings().ForEach(i => DiagnosticMessageHandler?.Invoke($"  {i}"));
        }
    }
}
