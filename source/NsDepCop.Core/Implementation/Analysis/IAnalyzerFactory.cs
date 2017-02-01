using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Provides analyzer objects.
    /// </summary>
    public interface IAnalyzerFactory
    {
        /// <summary>
        /// Returns a new dependency analyzer object using the specified config.
        /// </summary>
        /// <param name="config">The config of the analyzer.</param>
        /// <returns>A new dependency analyzer object using the specified config.</returns>
        IDependencyAnalyzer CreateDependencyAnalyzer(IProjectConfig config);
    }
}