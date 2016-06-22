using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private bool _configFileExists;
        private DateTime _configLastReadUtc;
        private Exception _configException;

        protected INsDepCopConfig Config;
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
            RefreshConfig();
        }

        public abstract string ParserName { get; }

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

                throw new Exception("Inconsistent DependencyAnalyzerState state.");
            }
        }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public abstract IEnumerable<DependencyViolation> AnalyzeProject(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths);

        /// <summary>
        /// Loads or refreshes config from file.
        /// </summary>
        public void RefreshConfig()
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

                    TypeDependencyValidator = new TypeDependencyValidator(
                        Config.AllowedDependencies,
                        Config.DisallowedDependencies,
                        Config.ChildCanDependOnParentImplicitly,
                        Config.VisibleTypesByNamespace);
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

        protected void EnsureValidStateForAnalysis()
        {
            if (State != DependencyAnalyzerState.Enabled)
                throw new InvalidOperationException($"Analyzer is in {State} state.");
        }

        protected void DebugDumpCacheStatistics()
        {
            foreach (var cacheStatistics in TypeDependencyValidator.GetCacheStatistics())
            {
                Debug.WriteLine(string.Format("{0} cache hits: {1}, misses:{2}, efficiency (hits/all): {3:P}",
                    cacheStatistics.CacheName, cacheStatistics.HitCount, cacheStatistics.MissCount, cacheStatistics.EfficiencyPercent),
                    Constants.TOOL_NAME);
            }
        }
    }
}