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
        /// <returns>A new dependency analyzer instance.</returns>
        IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath);
    }
}