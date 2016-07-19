using System.Collections.Generic;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Validates type dependencies to a set of allowed/disallowed rules and caches the results.
    /// </summary>
    public class CachingTypeDependencyValidator : TypeDependencyValidator, ICacheStatisticsProvider
    {
        private readonly Dictionary<TypeDependency, bool> _dependencyValidationCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingTypeDependencyValidator(IRuleConfig ruleConfig)
            : base(ruleConfig)
        {
            _dependencyValidationCache = new Dictionary<TypeDependency, bool>();
            HitCount = 0;
            MissCount = 0;
        }

        public double EfficiencyPercent => MathHelper.CalculatePercent(HitCount, HitCount + MissCount);

        public override bool IsAllowedDependency(TypeDependency typeDependency)
        {
            if (typeDependency.FromNamespaceName == typeDependency.ToNamespaceName)
                return true;

            bool isAllowedDependency;
            if (_dependencyValidationCache.TryGetValue(typeDependency, out isAllowedDependency))
            {
                HitCount++;

                Debug.WriteLine(
                    $"Cache hit: dependency {typeDependency} is {isAllowedDependency}.",
                    Constants.TOOL_NAME);
            }
            else
            {
                MissCount++;

                isAllowedDependency = base.IsAllowedDependency(typeDependency);
                _dependencyValidationCache.Add(typeDependency, isAllowedDependency);

                Debug.WriteLine($"Dependency {typeDependency} added to cache as {isAllowedDependency}.",
                    Constants.TOOL_NAME);
            }

            return isAllowedDependency;
        }
    }
}
