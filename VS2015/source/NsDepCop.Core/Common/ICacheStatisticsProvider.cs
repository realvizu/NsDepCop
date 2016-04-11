namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Provides cache statistics.
    /// </summary>
    public interface ICacheStatisticsProvider
    {
        int HitCount { get; }
        int MissCount { get; }
        double EfficiencyPercent { get; }
    }
}
