namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that analysis was disabled in the config file.
    /// </summary>
    public sealed class ConfigDisabledMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.ConfigDisabledIssue;
    }
}