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
            RefreshConfig();
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

                throw new Exception("Inconsistent DependencyAnalyzerState state.");
            }
        }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public IEnumerable<DependencyViolation> AnalyzeProject(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            EnsureValidStateForAnalysis();

            foreach (var dependencyViolation in AnalyzeProjectOverride(sourceFilePaths, referencedAssemblyPaths))
                yield return dependencyViolation;

            DebugDumpCacheStatistics(TypeDependencyValidator);
        }

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
                    TypeDependencyValidator = new CachingTypeDependencyValidator(Config);
                }
            }
            catch (Exception e)
            {
                _configException = e;
                Config = null;
            }
        }

        protected abstract IEnumerable<DependencyViolation> AnalyzeProjectOverride(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths);

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