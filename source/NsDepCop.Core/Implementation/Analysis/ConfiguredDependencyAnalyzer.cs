using System;
using System.Collections.Generic;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// A dependency analyzer with a refreshable config.
    /// </summary>
    /// <remarks>
    /// Uses read-writer lock to avoid config refresh while running analysis.
    /// </remarks>
    internal class ConfiguredDependencyAnalyzer : IConfiguredDependencyAnalyzer, IDisposable
    {
        private readonly IConfigProvider _configProvider;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly MessageHandler _traceMessageHandler;
        private readonly ReaderWriterLockSlim _configRefreshLock;

        private IAnalyzerConfig _config;
        private DependencyAnalyzer _dependencyAnalyzer;

        public ConfiguredDependencyAnalyzer(IConfigProvider configProvider, 
            ITypeDependencyEnumerator typeDependencyEnumerator, 
            MessageHandler traceMessageHandler)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _traceMessageHandler = traceMessageHandler;
            _configRefreshLock = new ReaderWriterLockSlim();

            UpdateConfig();
        }

        public IAnalyzerConfig Config => _config;
        public AnalyzerConfigState ConfigState => _configProvider.ConfigState;
        public Exception ConfigException => _configProvider.ConfigException;

        public int HitCount => _dependencyAnalyzer?.HitCount ?? 0;
        public int MissCount => _dependencyAnalyzer?.MissCount ?? 0;
        public double EfficiencyPercent => _dependencyAnalyzer?.EfficiencyPercent ?? 0;

        public void Dispose()
        {
            _configRefreshLock?.Dispose();
        }

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            _configRefreshLock.EnterReadLock();

            try
            {
                EnsureValidStateForAnalysis();
                return _dependencyAnalyzer.AnalyzeProject(sourceFilePaths, referencedAssemblyPaths);
            }
            finally
            {
                _configRefreshLock.ExitReadLock();
            }
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            _configRefreshLock.EnterReadLock();

            try
            {
                EnsureValidStateForAnalysis();
                return _dependencyAnalyzer.AnalyzeSyntaxNode(syntaxNode, semanticModel);
            }
            finally
            {
                _configRefreshLock.ExitReadLock();
            }
        }

        public void RefreshConfig()
        {
            _configRefreshLock.EnterWriteLock();

            try
            {
                _configProvider.RefreshConfig();
                UpdateConfig();
            }
            finally
            {
                _configRefreshLock.ExitWriteLock();
            }
        }

        private void UpdateConfig()
        {
            var oldConfig = _config;
            _config = _configProvider.Config;

            if (oldConfig == _config)
                return;

            UpdateAnalyzer();
        }

        private void UpdateAnalyzer()
        {
            _dependencyAnalyzer = ConfigState == AnalyzerConfigState.Enabled 
                ? new DependencyAnalyzer(Config, _typeDependencyEnumerator, _traceMessageHandler) 
                : null;
        }

        private void EnsureValidStateForAnalysis()
        {
            if (_configProvider.ConfigState != AnalyzerConfigState.Enabled)
                throw new InvalidOperationException($"Cannot analyze project because the analyzer state is {_configProvider.ConfigState}.");
        }
    }
}
