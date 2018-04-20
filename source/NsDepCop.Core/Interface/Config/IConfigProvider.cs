using System;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Provides config info and config state.
    /// </summary>
    public interface IConfigProvider
    {
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
        /// Reloads the config from its repository.
        /// </summary>
        void RefreshConfig();

        /// <summary>
        /// Updates the MaxIssueCount config parameter to the given value and persists the changes.
        /// </summary>
        /// <param name="newValue">The new value for MaxIssueCount.</param>
        void UpdateMaxIssueCount(int newValue);
    }
}
