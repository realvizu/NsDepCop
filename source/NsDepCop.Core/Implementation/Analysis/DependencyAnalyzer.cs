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
    internal class DependencyAnalyzer : IDependencyAnalyzer
    {
        private readonly ReaderWriterLockSlim _configRefreshLock;
        private readonly IConfigProvider _configProvider;
        private readonly Action<string> _diagnosticMessageHandler;

        private IAnalyzerConfig _config;
        private CachingTypeDependencyValidator _typeDependencyValidator;
        private ITypeDependencyEnumerator _typeDependencyEnumerator;

        public DependencyAnalyzer(IConfigProvider configProvider, Action<string> diagnosticMessageHandler = null)
        {
            _configRefreshLock = new ReaderWriterLockSlim();
            _configProvider = configProvider;
            _diagnosticMessageHandler = diagnosticMessageHandler;

            UpdateConfig();
        }

        public IAnalyzerConfig Config => _config;
        public AnalyzerState State => _configProvider.State;
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

            if (oldConfig != _config)
            {
                _diagnosticMessageHandler?.Invoke($"Config updated by {_configProvider}");
                UpdateAnalyzerLogic();
            }
        }

        private void UpdateAnalyzerLogic()
        {
            if (State == AnalyzerState.Enabled)
            {
                _typeDependencyValidator = new CachingTypeDependencyValidator(_config, _diagnosticMessageHandler);
                _typeDependencyEnumerator = TypeDependencyEnumeratorFactory.Create(_config.Parser);
            }
            else
            {
                _typeDependencyEnumerator = null;
            }
        }
    }
}
