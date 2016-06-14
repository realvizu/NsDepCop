using System.Collections.Generic;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Abstract base class for dependency analyzer implementations.
    /// </summary>
    public abstract class DependencyAnalyzerBase : IDependencyAnalyzer
    {
        protected readonly NsDepCopConfig Config;
        protected readonly ITypeDependencyValidator TypeDependencyValidator;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="config">Config object.</param>
        /// <param name="typeDependencyValidator"></param>
        protected DependencyAnalyzerBase(NsDepCopConfig config, ITypeDependencyValidator typeDependencyValidator)
        {
            Config = config;
            TypeDependencyValidator = typeDependencyValidator;            
        }

        public abstract string ParserName { get; }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        public abstract IEnumerable<DependencyViolation> AnalyzeProject(
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