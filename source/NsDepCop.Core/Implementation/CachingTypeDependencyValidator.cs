using System.Collections.Concurrent;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Validates type dependencies to a set of allowed/disallowed rules and caches the results.
    /// </summary>
    public class CachingTypeDependencyValidator : TypeDependencyValidator, ICacheStatisticsProvider
    {
        private readonly ConcurrentDictionary<TypeDependency, bool> _dependencyValidationCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingTypeDependencyValidator(IRuleConfig ruleConfig)
            : base(ruleConfig)
        {
            _dependencyValidationCache = new ConcurrentDictionary<TypeDependency, bool>();
            HitCount = 0;
            MissCount = 0;
        }

        public double EfficiencyPercent => MathHelper.CalculatePercent(HitCount, HitCount + MissCount);

        public override bool IsAllowedDependency(TypeDependency typeDependency)
        {
            if (typeDependency.FromNamespaceName == typeDependency.ToNamespaceName)
                return true;

            bool added;
            var isAllowedDependency = _dependencyValidationCache.GetOrAdd(typeDependency, base.IsAllowedDependency, out added);

            if (added)
            {
                MissCount++;

                Debug.WriteLine($"Dependency {typeDependency} added to cache as {isAllowedDependency}.",
                    Constants.TOOL_NAME);
            }
            else
            {
                HitCount++;

                Debug.WriteLine(
                    $"Cache hit: dependency {typeDependency} is {isAllowedDependency}.",
                    Constants.TOOL_NAME);
            }

            return isAllowedDependency;
        }
    }
}
