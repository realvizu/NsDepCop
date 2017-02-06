using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Finds illegal type dependencies according to a dependency rule set.
    /// </summary>
    internal class DependencyAnalyzer : IDependencyAnalyzer
    {
        private readonly ReaderWriterLockSlim _configRefreshLock;
        private readonly IConfigProvider _configProvider;

        private IAnalyzerConfig _config;
        private ITypeDependencyValidator _typeDependencyValidator;
        private ITypeDependencyEnumerator _typeDependencyEnumerator;

        public DependencyAnalyzer(IConfigProvider configProvider)
        {
            _configRefreshLock = new ReaderWriterLockSlim();
            _configProvider = configProvider;

            UpdateConfig();
        }

        public IAnalyzerConfig Config => _config;
        public ConfigState ConfigState => _configProvider.ConfigState;
        public Exception ConfigException => _configProvider.ConfigException;

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

        public ICacheStatisticsProvider GetCacheStatistics() => _typeDependencyValidator as ICacheStatisticsProvider;

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
                UpdateAnalyzerLogic();
            }
        }

        private void UpdateAnalyzerLogic()
        {
            if (ConfigState == ConfigState.Enabled)
            {
                _typeDependencyValidator = new CachingTypeDependencyValidator(_config);
                _typeDependencyEnumerator = TypeDependencyEnumeratorFactory.Create(_config.Parser);
            }
            else
            {
                _typeDependencyEnumerator = null;
            }
        }
    }
}
