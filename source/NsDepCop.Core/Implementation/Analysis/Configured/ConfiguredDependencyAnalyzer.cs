using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Configured;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Configured
{
    /// <summary>
    /// A dependency analyzer bundled together with its config provider.
    /// The config can be refreshed and updated.
    /// </summary>
    /// <remarks>
    /// Uses read-writer lock to avoid config refresh and update while running analysis.
    /// </remarks>
    internal class ConfiguredDependencyAnalyzer : IConfiguredDependencyAnalyzer, IDisposable
    {
        private readonly IUpdateableConfigProvider _configProvider;
        private readonly Func<IDependencyAnalyzer> _dependencyAnalyzerCreateFunc;
        private readonly ReaderWriterLockSlim _configReadWriteRefreshLock;

        private IAnalyzerConfig _config;
        private IDependencyAnalyzer _dependencyAnalyzer;

        public ConfiguredDependencyAnalyzer(IUpdateableConfigProvider configProvider, Func<IDependencyAnalyzer> dependencyAnalyzerCreateFunc)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _dependencyAnalyzerCreateFunc = dependencyAnalyzerCreateFunc ?? throw new ArgumentNullException(nameof(dependencyAnalyzerCreateFunc));
            _configReadWriteRefreshLock = new ReaderWriterLockSlim();

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
            _configReadWriteRefreshLock?.Dispose();
        }

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            _configReadWriteRefreshLock.EnterUpgradeableReadLock();
            try
            {
                EnsureValidStateForAnalysis();
                var illegalDependencies = _dependencyAnalyzer.AnalyzeProject(sourceFilePaths, referencedAssemblyPaths).ToList();
                LowerMaxIssueCountIfNeeded(illegalDependencies.Count);
                return illegalDependencies;
            }
            finally
            {
                _configReadWriteRefreshLock.ExitUpgradeableReadLock();
            }
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            _configReadWriteRefreshLock.EnterReadLock();
            try
            {
                EnsureValidStateForAnalysis();
                return _dependencyAnalyzer.AnalyzeSyntaxNode(syntaxNode, semanticModel);
            }
            finally
            {
                _configReadWriteRefreshLock.ExitReadLock();
            }
        }

        public void RefreshConfig()
        {
            _configReadWriteRefreshLock.EnterWriteLock();
            try
            {
                _configProvider.RefreshConfig();
                UpdateConfig();
            }
            finally
            {
                _configReadWriteRefreshLock.ExitWriteLock();
            }
        }

        private void UpdateMaxIssueCount(int newValue)
        {
            _configReadWriteRefreshLock.EnterWriteLock();
            try
            {
                _configProvider.UpdateMaxIssueCount(newValue);
                UpdateConfig();
            }
            finally
            {
                _configReadWriteRefreshLock.ExitWriteLock();
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
                ? _dependencyAnalyzerCreateFunc()
                : null;
        }

        private void EnsureValidStateForAnalysis()
        {
            if (_configProvider.ConfigState != AnalyzerConfigState.Enabled)
                throw new InvalidOperationException($"Cannot analyze project because the analyzer state is {_configProvider.ConfigState}.");
        }

        private void LowerMaxIssueCountIfNeeded(int illegalDependencyCount)
        {
            if (_config.AutoLowerMaxIssueCount && illegalDependencyCount < _config.MaxIssueCount)
            {
                UpdateMaxIssueCount(illegalDependencyCount);
            }
        }
    }
}
