﻿namespace Codartis.NsDepCop.Util
{
    /// <summary>
    /// Provides cache efficiency statistics.
    /// </summary>
    public interface ICacheStatisticsProvider
    {
        int HitCount { get; }
        int MissCount { get; }
        double EfficiencyPercent { get; }
    }
}
