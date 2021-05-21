using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message describing that too many issues were found.
    /// </summary>
    public sealed class TooManyIssuesMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.TooManyIssuesIssue;

        public int MaxIssueCount { get; }
        public override IssueKind IssueKind { get; }

        public TooManyIssuesMessage(int maxIssueCount, IssueKind issueKind)
        {
            MaxIssueCount = maxIssueCount;
            IssueKind = issueKind;
        }

        public override string ToString() => $"Max issue count ({MaxIssueCount}) reached, analysis was stopped.";
    }
}