using System;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates config provider objects.
    /// </summary>
    public class ConfigProviderFactory
    {
        /// <summary>
        /// Creates a config provider for an xml config file.
        /// </summary>
        /// <param name="configPath">The name and full path of the xml config file.</param>
        /// <returns>A new config provider instance.</returns>
        public IConfigProvider CreateFromXmlConfigFile(string configPath)
        {
            if (string.IsNullOrWhiteSpace(configPath))
                throw new ArgumentException("Config path must not be null or whitespace.", nameof(configPath));

            return new XmlFileConfigProvider(configPath);
        }
    }
}
