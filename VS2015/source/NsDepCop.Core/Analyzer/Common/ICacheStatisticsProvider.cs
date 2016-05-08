namespace Codartis.NsDepCop.Core.Analyzer.Common
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
