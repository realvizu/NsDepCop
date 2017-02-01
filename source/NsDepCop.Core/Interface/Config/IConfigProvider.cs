using System;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Provides config info from a repository.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// The config info extracted from the config repository.
        /// </summary>
        IProjectConfig Config { get; }

        /// <summary>
        /// Gets the state of the config.
        /// </summary>
        ConfigState ConfigState { get; }

        /// <summary>
        /// Gets the config exception (if any). Returns null if there was no exception.
        /// </summary>
        Exception ConfigException { get; }

        /// <summary>
        /// Reloads the config from its repository.
        /// </summary>
        void RefreshConfig();
    }
}
