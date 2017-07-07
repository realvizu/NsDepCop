using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        /// <summary>
        /// Creates a dependency analyzer from a config.
        /// </summary>
        /// <param name="config">An analyzer config.</param>
        /// <returns>A dependency analyzer.</returns>
        IDependencyAnalyzer Create(IAnalyzerConfig config);

        /// <summary>
        /// Creates a refreshable dependency analyzer for an xml config file.
        /// </summary>
        /// <param name="configFilePath">The full path of the xml config file.</param>
        /// <returns>A refreshable dependency analyzer.</returns>
        IRefreshableDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath);

        /// <summary>
        /// Creates a refreshable dependency analyzer object for the xml config files found in the specified folder and its parents.
        /// </summary>
        /// <param name="folderPath">The full path of the folder where the search for config files begins.</param>
        /// <returns>A refreshable dependency analyzer.</returns>
        IRefreshableDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath);

    }
}