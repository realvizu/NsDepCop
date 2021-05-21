using System;

namespace Codartis.NsDepCop.Interface.Analysis.Messages
{
    /// <summary>
    /// A message that describes a config exception.
    /// </summary>
    public sealed class ConfigErrorMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.ConfigExceptionIssue;

        public Exception Exception { get; }

        public ConfigErrorMessage(Exception exception)
        {
            Exception = exception;
        }

        public override string ToString() => $"{Exception?.Message}";
    }
}