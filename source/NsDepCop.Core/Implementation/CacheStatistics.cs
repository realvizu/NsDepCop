namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Encapsulates cache statistics information extracted from a cache statistics provider.
    /// </summary>
    public struct CacheStatistics
    {
        public string CacheName { get; }
        public int HitCount { get; }
        public int MissCount { get; }
        public double EfficiencyPercent { get; }

        public CacheStatistics(ICacheStatisticsProvider cacheStatisticsProvider)
        {
            CacheName = cacheStatisticsProvider.GetType().Name;
            HitCount = cacheStatisticsProvider.HitCount;
            MissCount = cacheStatisticsProvider.MissCount;
            EfficiencyPercent = cacheStatisticsProvider.EfficiencyPercent;
        }
    }
}
