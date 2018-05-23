using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message that describes a config exception.
    /// </summary>
    [Serializable]
    public class ConfigErrorMessage : IssueMessageBase
    {
        public Exception Exception { get; }

        public ConfigErrorMessage(Exception exception) 
            : base (IssueType.ConfigException, Config.IssueKind.Error)
        {
            Exception = exception;
        }

        public override string ToString()
            => IssueDefinitions.ConfigExceptionIssue.DescriptionFormatterDelegate.Invoke(Exception);
    }
}
