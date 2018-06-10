using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
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
        /// Logs the given issue.
        /// </summary>
        void LogIssue(IssueMessageBase issueMessage);

        /// <summary>
        /// Logs the given info message with the configured InfoImportance.
        /// </summary>
        void LogInfo(InfoMessageBase infoMessage);

        /// <summary>
        /// Logs the given message as an error.
        /// </summary>
        /// <param name="message">The message text.</param>
        void LogError(string message);

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
    }
}