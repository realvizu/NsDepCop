using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates configured dependency analyzer objects.
    /// </summary>
    public interface IConfiguredDependencyAnalyzerFactory
    {
        /// <summary>
        /// Creates a configured dependency analyzer for an xml config file.
        /// </summary>
        /// <param name="configFilePath">The full path of the xml config file.</param>
        /// <returns>A configured dependency analyzer.</returns>
        IConfiguredDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath);

        /// <summary>
        /// Creates a configured dependency analyzer object for the xml config files found in the specified folder and its parents.
        /// </summary>
        /// <param name="folderPath">The full path of the folder where the search for config files begins.</param>
        /// <returns>A configured dependency analyzer.</returns>
        IConfiguredDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath);
    }
}