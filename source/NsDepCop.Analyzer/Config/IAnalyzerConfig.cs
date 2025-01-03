using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// The configuration for an analyzer.
    /// </summary>
    public interface IAnalyzerConfig : IDependencyRules, IDiagnosticSupport
    {
        /// <summary>
        /// Gets a value indicating whether analysis is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Gets the max number of issues reported.
        /// </summary>
        int MaxIssueCount { get; }

        /// <summary>
        /// Gets a value indicating whether MaxIssueCount is automatically updated whenever a lower count is achieved.
        /// </summary>
        bool AutoLowerMaxIssueCount { get; }

        /// <summary>
        /// Gets an array of file path exclusions patterns. Source files that match any of these patterns won't be analyzed.
        /// </summary>
        /// <remarks>
        /// Uses https://github.com/dazinator/DotNet.Glob patterns.
        /// </remarks>
        string[] SourcePathExclusionPatterns { get; }

        /// <summary>
        /// Gets a value indicating whether the assembly dependencies check should be performed.
        /// </summary>
        bool CheckAssemblyDependencies { get; }
    }
}