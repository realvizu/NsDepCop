using System.Collections.Generic;
using System.Diagnostics;

namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Abstract base class for dependency analyzer implementations.
    /// </summary>
    public abstract class DependencyAnalyzerBase : IDependencyAnalyzer
    {
        protected readonly NsDepCopConfig Config;
        protected readonly TypeDependencyValidator TypeDependencyValidator;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="config">Config object.</param>
        protected DependencyAnalyzerBase(NsDepCopConfig config)
        {
            Config = config;
            TypeDependencyValidator = new TypeDependencyValidator(
                config.AllowedDependencies, 
                config.DisallowedDependencies,
                config.ChildCanDependOnParentImplicitly, 
                config.VisibleTypesByNamespace);
        }

        public abstract string ParserName { get; }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="baseDirectory">The full path of the base directory of the project.</param>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public abstract IEnumerable<DependencyViolation> AnalyzeProject(
            string baseDirectory,
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths);

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