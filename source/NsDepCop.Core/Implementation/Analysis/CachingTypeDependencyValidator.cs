using System.Collections.Concurrent;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Validates type dependencies to a set of allowed/disallowed rules and caches the results.
    /// </summary>
    public class CachingTypeDependencyValidator : TypeDependencyValidator, ICacheStatisticsProvider
    {
        private readonly ConcurrentDictionary<TypeDependency, bool> _dependencyValidationCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingTypeDependencyValidator(IDependencyRules dependencyRules)
            : base(dependencyRules)
        {
            _dependencyValidationCache = new ConcurrentDictionary<TypeDependency, bool>();
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
                    ProductConstants.ToolName);
            }
            else
            {
                HitCount++;

                Debug.WriteLine(
                    $"Cache hit: dependency {typeDependency} is {isAllowedDependency}.",
                    ProductConstants.ToolName);
            }

            return isAllowedDependency;
        }
    }
}
