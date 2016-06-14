using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation.Common
{
    /// <summary>
    /// Validates dependencies to a set of type visibility definitions and caches the results.
    /// </summary>
    public class CachingTypeVisibilityValidator : TypeVisibilityValidator, ICacheStatisticsProvider
    {
        private readonly Dictionary<string, bool> _typeVisibilityCache;

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }

        public CachingTypeVisibilityValidator(ImmutableDictionary<string, ImmutableHashSet<string>> visibleTypesPerNamespaces)
            : base(visibleTypesPerNamespaces)
        {
            _typeVisibilityCache = new Dictionary<string, bool>();

            HitCount = 0;
            MissCount = 0;
        }

        public double EfficiencyPercent => MathHelper.CalculatePercent(HitCount, HitCount + MissCount);

        public override bool IsTypeVisible(string namespaceName, string typeName)
        {
            var cacheKey = CreateCacheKey(namespaceName, typeName);
            bool isTypeVisible;
            if (_typeVisibilityCache.TryGetValue(cacheKey, out isTypeVisible))
            {
                HitCount++;

                Debug.WriteLine(
                    $"Cache hit: type {namespaceName}.{typeName} is {IsVisibleToString(isTypeVisible)}.",
                    Constants.TOOL_NAME);
            }
            else
            {
                MissCount++;

                isTypeVisible = base.IsTypeVisible(namespaceName, typeName);

                _typeVisibilityCache.Add(cacheKey, isTypeVisible);

                Debug.WriteLine($"{cacheKey} added to cache as {IsVisibleToString(isTypeVisible)}.",
                    Constants.TOOL_NAME);
            }

            return isTypeVisible;
        }

        private static string CreateCacheKey(string namespaceName, string typeName) 
            => $"{namespaceName}.{typeName}";
    }
}
