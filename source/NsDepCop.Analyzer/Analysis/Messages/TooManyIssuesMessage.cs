namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message describing that too many issues were found.
    /// </summary>
    public sealed class TooManyIssuesMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.TooManyIssuesIssue;

        public int MaxIssueCount { get; }

        public TooManyIssuesMessage(int maxIssueCount)
        {
            MaxIssueCount = maxIssueCount;
        }

        public override string ToString() => $"Max issue count ({MaxIssueCount}) reached, analysis was stopped.";
    }
}