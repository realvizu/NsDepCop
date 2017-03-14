using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        /// <summary>
        /// Creates a dependency analyzer object for an xml config file.
        /// </summary>
        /// <param name="configFilePath">The full path of the xml config file.</param>
        /// <returns>A configured dependency analyzer instance.</returns>
        IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath);

        /// <summary>
        /// Creates a dependency analyzer object for the xml config files found in the specified folder and its parents.
        /// </summary>
        /// <param name="folderPath">The full path of the folder where the search for config files begins.</param>
        /// <returns>A configured dependency analyzer instance.</returns>
        IDependencyAnalyzer CreateFromMultiLevelXmlConfigFile(string folderPath);

    }
}