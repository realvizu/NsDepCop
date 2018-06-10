using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.Build.Framework;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Logs issues and messages using the MsBuild engine.
    /// </summary>
    public sealed class MsBuildLoggerGateway : ILogger
    {
        private readonly IBuildEngine _buildEngine;

        public MessageImportance InfoImportance { get; set; }

        public MsBuildLoggerGateway(IBuildEngine buildEngine)
        {
            _buildEngine = buildEngine ?? throw new ArgumentNullException(nameof(buildEngine));
            InfoImportance = MessageImportance.Normal;
        }

        public void LogTraceMessage(IEnumerable<string> messages)
        {
            foreach (var message in messages)
                LogTraceMessage(message);
        }

        public void LogTraceMessage(string message) => LogBuildEvent(IssueKind.Info, message, MessageImportance.Low);

        public void LogError(string message) => LogBuildEvent(IssueKind.Error, message);

        public void LogInfo(InfoMessageBase infoMessage) => LogBuildEvent(IssueKind.Info, infoMessage.ToString(), InfoImportance);

        public void LogIssue(IssueMessageBase issueMessage)
        {
            var issueKind = issueMessage.IssueKind;
            var message = issueMessage.ToString();
            var code = issueMessage.Code;

            var sourceSegment = issueMessage.SourceSegment;
            var path = sourceSegment?.Path;
            var startLine = sourceSegment?.StartLine ?? 0;
            var startColumn = sourceSegment?.StartColumn ?? 0;
            var endLine = sourceSegment?.EndLine ?? 0;
            var endColumn = sourceSegment?.EndColumn ?? 0;

            LogBuildEvent(issueKind, message, InfoImportance, code, path, startLine, startColumn, endLine, endColumn);
        }

        private void LogBuildEvent(IssueKind issueKind, string message, MessageImportance messageImportance = MessageImportance.Normal, string code = null,
            string path = null, int startLine = 0, int startColumn = 0, int endLine = 0, int endColumn = 0)
        {
            switch (issueKind)
            {
                case IssueKind.Error:
                    _buildEngine.LogErrorEvent(new BuildErrorEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName));
                    break;

                case IssueKind.Warning:
                    _buildEngine.LogWarningEvent(new BuildWarningEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, message, code, ProductConstants.ToolName));
                    break;

                default:
                    var formattedMessage = $"[{ProductConstants.ToolName}] {message}";
                    _buildEngine.LogMessageEvent(new BuildMessageEventArgs(
                        null, code, path, startLine, startColumn, endLine, endColumn, formattedMessage, code, ProductConstants.ToolName, messageImportance));
                    break;
            }
        }
    }
}