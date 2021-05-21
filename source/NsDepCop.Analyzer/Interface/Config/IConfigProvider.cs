using System;

namespace Codartis.NsDepCop.Interface.Config
{
    /// <summary>
    /// Provides config info and config state from some kind of repository.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Gets the importance of the info messages.
        /// </summary>
        Importance InfoImportance { get; }

        /// <summary>
        /// Gets the config used by the analyzers.
        /// </summary>
        IAnalyzerConfig Config { get; }

        /// <summary>
        /// Gets the state of the analyzer's config.
        /// </summary>
        AnalyzerConfigState ConfigState { get; }

        /// <summary>
        /// Gets the config exception or null if there was no exception.
        /// </summary>
        Exception ConfigException { get; }

        /// <summary>
        /// Gets the config location as a string.
        /// </summary>
        string ConfigLocation { get; }

        /// <summary>
        /// Reloads the config from its repository.
        /// </summary>
        void RefreshConfig();
    }
}
