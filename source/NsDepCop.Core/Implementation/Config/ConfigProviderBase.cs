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
        protected readonly Action<string> DiagnosticMessageHandler;

        private readonly object _lockObject = new object();
        private bool _isInitialized;

        private IAnalyzerConfig _config;
        private Exception _configException;

        protected ConfigProviderBase(Action<string> diagnosticMessageHandler = null)
        {
            DiagnosticMessageHandler = diagnosticMessageHandler;
        }

        protected bool IsConfigLoaded => _config != null;
        protected bool IsConfigErroneous => _configException != null;

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

                    return GetState();
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
                try
                {
                    _config = GetConfig();
                    DumpConfigToDiagnosticOutput();
                }
                catch (Exception e)
                {
                    _configException = e;
                    DiagnosticMessageHandler?.Invoke($"RefreshConfig exception: {e}");
                }
            }
        }

        protected abstract IAnalyzerConfig GetConfig();
        protected abstract AnalyzerState GetState();

        private void Initialize()
        {
            _isInitialized = true;
            RefreshConfig();
        }

        private void DumpConfigToDiagnosticOutput()
        {
            _config?.DumpToStrings().ForEach(i => DiagnosticMessageHandler?.Invoke($"  {i}"));
        }
    }
}
