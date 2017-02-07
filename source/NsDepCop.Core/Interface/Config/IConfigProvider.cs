using System;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Provides config info and config state.
    /// </summary>
    public interface IConfigProvider
    {
        /// <summary>
        /// Get the config used by the analyzers.
        /// </summary>
        IAnalyzerConfig Config { get; }

        /// <summary>
        /// Gets the state of the analyzer.
        /// </summary>
        AnalyzerState State { get; }

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
