using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Validates dependencies to a set of allowed/disallowed rules and caches the results.
    /// </summary>
    public class CachingNamespaceDependencyValidator : NamespaceDependencyValidator, ICacheStatisticsProvider
    {
        private readonly Dictionary<Dependency, bool> _dependencyValidationCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingNamespaceDependencyValidator(
            ImmutableHashSet<Dependency> allowedDependencies,
            ImmutableHashSet<Dependency> disallowedDependencies,
            bool childCanDependOnParentImplicitly = false)
            : base(allowedDependencies, disallowedDependencies, childCanDependOnParentImplicitly)
        {
            _dependencyValidationCache = new Dictionary<Dependency, bool>();
            HitCount = 0;
            MissCount = 0;
        }

        public double EfficiencyPercent => MathHelper.CalculatePercent(HitCount, HitCount + MissCount);

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public override bool IsAllowedDependency(string fromNamespace, string toNamespace)
        {
            if (fromNamespace == toNamespace)
                return true;

            var dependency = new Dependency(fromNamespace, toNamespace);

            bool isAllowedDependency;
            if (_dependencyValidationCache.TryGetValue(dependency, out isAllowedDependency))
            {
                HitCount++;

                Debug.WriteLine(
                    $"Cache hit: dependency {dependency} is {IsAllowedToString(isAllowedDependency)}.",
                    Constants.TOOL_NAME);
            }
            else
            {
                MissCount++;

                isAllowedDependency = base.IsAllowedDependency(fromNamespace, toNamespace);

                _dependencyValidationCache.Add(dependency, isAllowedDependency);

                Debug.WriteLine($"Dependency {dependency} added to cache as {IsAllowedToString(isAllowedDependency)}.",
                    Constants.TOOL_NAME);
            }

            return isAllowedDependency;
        }
    }
}
