namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message indicating that analysis was disabled with environment variable.
    /// </summary>
    public sealed class ToolDisabledMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.ToolDisabledIssue;
    }
}