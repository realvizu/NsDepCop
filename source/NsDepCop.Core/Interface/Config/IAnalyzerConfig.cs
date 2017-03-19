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
        /// Gets the severity of the reported issues.
        /// </summary>
        IssueKind IssueKind { get; }

        /// <summary>
        /// Gets the max number of issues reported.
        /// </summary>
        int MaxIssueCount { get; }

        /// <summary>
        /// Gets the importance level of information messages. 
        /// Influences whether messages are emitted or suppressed by the host.
        /// </summary>
        Importance InfoImportance { get; }

        /// <summary>
        /// The type of parser used by the analyzer.
        /// </summary>
        Parsers Parser { get; }
    }
}