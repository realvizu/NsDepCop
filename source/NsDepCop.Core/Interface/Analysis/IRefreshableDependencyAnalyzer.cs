using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Analyzes dependencies in source code based on a config. The config can be refreshed.
    /// </summary>
    public interface IRefreshableDependencyAnalyzer : IDependencyAnalyzer, IConfigProvider
    {
    }
}