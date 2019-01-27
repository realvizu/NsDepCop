using System;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Interface.Config
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
        /// Gets the severity of the dependency issues.
        /// </summary>
        IssueKind DependencyIssueSeverity { get; }

        /// <summary>
        /// Gets the max number of issues reported.
        /// </summary>
        int MaxIssueCount { get; }

        /// <summary>
        /// Gets the severity of reaching the max issue count.
        /// </summary>
        IssueKind MaxIssueCountSeverity { get; }

        /// <summary>
        /// Gets a value indicating whether MaxIssueCount is automatically updated whenever a lower count is achieved.
        /// </summary>
        bool AutoLowerMaxIssueCount { get; }

        /// <summary>
        /// Gets the importance level of information messages. 
        /// Influences whether messages are emitted or suppressed by the host.
        /// </summary>
        Importance InfoImportance { get; }

        /// <summary>
        /// Gets an array of time spans used as wait intervals when calling the analyzer service.
        /// The analyzer client will retry a failed call by using the next time span from this array until it runs out of array items.
        /// </summary>
        TimeSpan[] AnalyzerServiceCallRetryTimeSpans { get; }

        /// <summary>
        /// Gets an array of file path exclusions patterns. Source files that match any of these patterns won't be analyzed.
        /// </summary>
        /// <remarks>
        /// Uses https://github.com/dazinator/DotNet.Glob patterns.
        /// </remarks>
        string[] SourcePathExclusionPatterns { get; }
    }
}