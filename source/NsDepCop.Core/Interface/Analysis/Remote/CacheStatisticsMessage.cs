using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// A message that contains cache statistics.
    /// </summary>
    [Serializable]
    public class CacheStatisticsMessage : AnalyzerMessageBase
    {
        public int HitCount { get; }
        public int MissCount { get; }
        public double EfficiencyPercent { get; }

        public CacheStatisticsMessage(int hitCount, int missCount, double efficiencyPercent)
        {
            HitCount = hitCount;
            MissCount = missCount;
            EfficiencyPercent = efficiencyPercent;
        }
    }
}
