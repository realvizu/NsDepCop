using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Finds illegal type dependencies according to a dependency rule set.
    /// </summary>
    /// <remarks>
    /// Uses read-writer lock to avoid config refresh while running analysis.
    /// </remarks>
    internal class DependencyAnalyzer : IDependencyAnalyzer
    {
        private readonly IConfigProvider _configProvider;
        private readonly ReaderWriterLockSlim _configRefreshLock;
        private readonly Action<string> _diagnosticMessageHandler;

        private IAnalyzerConfig _config;
        private CachingTypeDependencyValidator _typeDependencyValidator;
        private ITypeDependencyEnumerator _typeDependencyEnumerator;

        public DependencyAnalyzer(IConfigProvider configProvider, Action<string> diagnosticMessageHandler = null)
        {
            if (configProvider == null)
                throw new ArgumentNullException(nameof(configProvider));

            _configProvider = configProvider;
            _configRefreshLock = new ReaderWriterLockSlim();
            _diagnosticMessageHandler = diagnosticMessageHandler;

            UpdateConfig();
        }

        public IAnalyzerConfig Config => _config;
        public AnalyzerConfigState ConfigState => _configProvider.ConfigState;
        public Exception ConfigException => _configProvider.ConfigException;

        public int HitCount => _typeDependencyValidator?.HitCount ?? 0;
        public int MissCount => _typeDependencyValidator?.MissCount ?? 0;
        public double EfficiencyPercent => _typeDependencyValidator?.EfficiencyPercent ?? 0;

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
                return GetIllegalDependencies(_typeDependencyEnumerator.GetTypeDependencies(sourceFilePaths, referencedAssemblyPaths));
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
                return GetIllegalDependencies(_typeDependencyEnumerator.GetTypeDependencies(syntaxNode, semanticModel));
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

        private IEnumerable<TypeDependency> GetIllegalDependencies(IEnumerable<TypeDependency> typeDependencies)
        {
            return typeDependencies.Where(i => !_typeDependencyValidator.IsAllowedDependency(i)).Take(Config.MaxIssueCount);
        }

        private void UpdateConfig()
        {
            var oldConfig = _config;
            _config = _configProvider.Config;

            if (oldConfig == _config)
                return;

            UpdateAnalyzerLogic();
        }

        private void UpdateAnalyzerLogic()
        {
            if (ConfigState == AnalyzerConfigState.Enabled)
            {
                _typeDependencyValidator = new CachingTypeDependencyValidator(_config, _diagnosticMessageHandler);
                _typeDependencyEnumerator = TypeDependencyEnumeratorFactory.Create(_config.Parser);
            }
            else
            {
                _typeDependencyEnumerator = null;
            }
        }

        private void EnsureValidStateForAnalysis()
        {
            if (_configProvider.ConfigState != AnalyzerConfigState.Enabled)
                throw new InvalidOperationException($"Cannot analyze project because the analyzer state is {_configProvider.ConfigState}.");
        }
    }
}
