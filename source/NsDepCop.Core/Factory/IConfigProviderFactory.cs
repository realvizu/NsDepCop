using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates config provider objects.
    /// </summary>
    public interface IConfigProviderFactory : IConfigInitializer<ConfigProviderFactory>
    {
        /// <summary>
        /// Creates a config provider for an xml config file.
        /// </summary>
        /// <param name="configFilePath">The full path of an xml config file.</param>
        /// <returns>A config provider.</returns>
        IUpdateableConfigProvider CreateFromXmlConfigFile(string configFilePath);

        /// <summary>
        /// Creates a multi level config provider for the xml config files found in the specified folder and its parents.
        /// </summary>
        /// <param name="folderPath">The full path of the folder where the search for config files begins.</param>
        /// <returns>A config provider.</returns>
        IUpdateableConfigProvider CreateFromMultiLevelXmlConfigFile(string folderPath);
    }
}