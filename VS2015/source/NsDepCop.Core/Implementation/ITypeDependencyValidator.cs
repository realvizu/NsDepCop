using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Determines whether a type-to-type dependency is allowed or not.
    /// </summary>
    public interface ITypeDependencyValidator
    {
        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="sourceType">The name of the source type of the dependency.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <param name="targeType">The name of the type that the dependency points to.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        bool IsAllowedDependency(string fromNamespace, string sourceType, string toNamespace, string targeType);

        /// <summary>
        /// Returns statistics about the effectiveness of the validator's cache.
        /// </summary>
        /// <returns>Statistics about the effectiveness of the validator's cache.</returns>
        IEnumerable<CacheStatistics> GetCacheStatistics();
    }
}