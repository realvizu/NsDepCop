using System.Collections.Concurrent;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    /// <summary>
    /// Validates type dependencies to a set of allowed/disallowed rules and caches the results.
    /// </summary>
    public class CachingTypeDependencyValidator : TypeDependencyValidator, ICacheStatisticsProvider
    {
        private readonly MessageHandler _traceMessageHandler;
        private readonly ConcurrentDictionary<TypeDependency, DependencyStatus> _dependencyValidationCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingTypeDependencyValidator(IDependencyRules dependencyRules, MessageHandler traceMessageHandler)
            : base(dependencyRules)
        {
            _traceMessageHandler = traceMessageHandler;
            _dependencyValidationCache = new ConcurrentDictionary<TypeDependency, DependencyStatus>();
        }

        public double EfficiencyPercent => MathHelper.CalculatePercent(HitCount, HitCount + MissCount);
        
        public override DependencyStatus IsAllowedDependency(TypeDependency typeDependency)
        {
            if (typeDependency.FromNamespaceName == typeDependency.ToNamespaceName)
                return DependencyStatus.Allowed;

            var isAllowedDependency = _dependencyValidationCache.GetOrAdd(typeDependency, base.IsAllowedDependency, out var added);

            if (added)
            {
                MissCount++;
                LogTraceMessage($"Dependency {typeDependency} added to cache as {isAllowedDependency}.");
            }
            else
            {
                HitCount++;
            }

            return isAllowedDependency;
        }

        private void LogTraceMessage(string message) => _traceMessageHandler?.Invoke(message);
    }
}
