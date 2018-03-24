using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Configured
{
    /// <summary>
    /// A dependency analyzer that also manages its config.
    /// </summary>
    /// <remarks>
    /// The config can be refreshed.
    /// </remarks>
    public interface IConfiguredDependencyAnalyzer : IDependencyAnalyzer, IConfigProvider
    {
    }
}