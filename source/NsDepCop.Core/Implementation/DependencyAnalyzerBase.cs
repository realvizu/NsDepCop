using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Abstract base class for dependency analyzer implementations.
    /// Handles config loading and refreshing.
    /// </summary>
    public abstract class DependencyAnalyzerBase : IDependencyAnalyzer
    {
        private readonly string _configFileName;
        private readonly ReaderWriterLockSlim _configRefreshLock;
        private bool _configFileExists;
        private DateTime _configLastReadUtc;
        private Exception _configException;

        protected NsDepCopConfig Config;
        protected ITypeDependencyValidator TypeDependencyValidator;

        private bool IsConfigLoaded => Config != null;
        private bool IsConfigErroneous => _configException != null;

        public string ConfigFileName => _configFileName;
        public Exception ConfigException => _configException;
        public IssueKind DependencyViolationIssueKind => IsConfigLoaded ? Config.IssueKind : IssueKind.Error;
        public int MaxIssueCount => IsConfigLoaded ? Config.MaxIssueCount : 0;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="configFileName">The name and full path of the config file required by the analyzer.</param>
        protected DependencyAnalyzerBase(string configFileName)
        {
            _configFileName = configFileName;
            _configRefreshLock = new ReaderWriterLockSlim();
            RefreshConfig();
        }

        public void Dispose()
        {
            _configRefreshLock.Dispose();
        }

        /// <summary>
        /// Gets the name of the parser currently used.
        /// </summary>
        public abstract string ParserName { get; }

        /// <summary>
        /// Gets the state of the analyzer object.
        /// </summary>
        public DependencyAnalyzerState State
        {
            get
            {
                if (!_configFileExists)
                    return DependencyAnalyzerState.NoConfigFile;

                if (IsConfigLoaded && !Config.IsEnabled)
                    return DependencyAnalyzerState.Disabled;

                if (IsConfigLoaded && Config.IsEnabled)
                    return DependencyAnalyzerState.Enabled;

                if (!IsConfigLoaded && IsConfigErroneous)
                    return DependencyAnalyzerState.ConfigError;

                throw new Exception("Inconsistent DependencyAnalyzer state.");
            }
        }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        /// <remarks>Mutually exclusive with RefreshConfig, guarded by ReaderWriterLock.</remarks>
        public IEnumerable<DependencyViolation> AnalyzeProject(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            _configRefreshLock.EnterReadLock();

            try
            {
                EnsureValidStateForAnalysis();

                foreach (var dependencyViolation in AnalyzeProjectOverride(sourceFilePaths, referencedAssemblyPaths))
                    yield return dependencyViolation;

                DebugDumpCacheStatistics(TypeDependencyValidator);
            }
            finally
            {
                _configRefreshLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Loads or refreshes config from file.
        /// </summary>
        /// <remarks>Mutually exclusive with AnalyzeProject, guarded by ReaderWriterLock.</remarks>
        public void RefreshConfig()
        {
            _configRefreshLock.EnterWriteLock();

            try
            {
                RefreshConfigPrivate();
            }
            finally
            {
                _configRefreshLock.ExitWriteLock();
            }
        }

        protected abstract IEnumerable<DependencyViolation> AnalyzeProjectOverride(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths);

        private void RefreshConfigPrivate()
        {
            _configFileExists = File.Exists(_configFileName);

            if (!_configFileExists)
            {
                _configException = null;
                Config = null;
                return;
            }

            try
            {
                // Read the config if it was never read, or whenever the file changes.
                if (!IsConfigLoaded || ConfigModifiedSinceLastRead())
                {
                    _configLastReadUtc = DateTime.UtcNow;

                    _configException = null;
                    Config = new NsDepCopConfig(_configFileName);
                    TypeDependencyValidator = new CachingTypeDependencyValidator(Config);
                }
            }
            catch (Exception e)
            {
                _configException = e;
                Config = null;
            }
        }

        private bool ConfigModifiedSinceLastRead()
        {
            return _configLastReadUtc < File.GetLastWriteTimeUtc(_configFileName);
        }

        private void EnsureValidStateForAnalysis()
        {
            if (State != DependencyAnalyzerState.Enabled)
                throw new InvalidOperationException($"Analyzer is in {State} state.");
        }

        private static void DebugDumpCacheStatistics(object o)
        {
            var cache = o as ICacheStatisticsProvider;
            if (cache == null)
                return;

            Debug.WriteLine($"Cache hits: {cache.HitCount}, misses:{cache.MissCount}, efficiency (hits/all): {cache.EfficiencyPercent:P}",
                Constants.TOOL_NAME);
        }
    }
}