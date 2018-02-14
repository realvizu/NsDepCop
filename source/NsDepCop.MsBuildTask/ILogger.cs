using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.Build.Framework;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Provides logging operations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets or sets the importance of the info messages.
        /// </summary>
        MessageImportance InfoImportance { get; set; }

        /// <summary>
        /// Logs the given issue with its static description.
        /// </summary>
        /// <param name="issueDescriptor">Describes the issue's properties (severity, description).</param>
        void LogIssue(IssueDescriptor issueDescriptor);

        /// <summary>
        /// Logs the given issue with a dynamic message using the given issue parameter.
        /// </summary>
        /// <typeparam name="T">The type of the issue's parameter.</typeparam>
        /// <param name="issueDescriptor">Describes the issue's properties (severity, description pattern)</param>
        /// <param name="issueParameter">The parameter of the issue. Will be formatted into the issue's description.</param>
        /// <param name="issueKindOverride">The severity of the issue. Optional. If specified then overrides the issue's default severity.</param>
        /// <param name="sourceSegment">The source segment where the issue was found. Optional.</param>
        void LogIssue<T>(IssueDescriptor<T> issueDescriptor, T issueParameter = default(T), 
            IssueKind? issueKindOverride = null, SourceSegment? sourceSegment = null);

        /// <summary>
        /// Log a collection of trace messages. 
        /// </summary>
        /// <param name="messages">A message collection.</param>
        void LogTraceMessage(IEnumerable<string> messages);

        /// <summary>
        /// Logs a trace message. 
        /// </summary>
        /// <param name="message">A string message.</param>
        void LogTraceMessage(string message);

        void SetMaxWarningErrorThreshold(int? maxWarningErrorThreshold);
    }
}