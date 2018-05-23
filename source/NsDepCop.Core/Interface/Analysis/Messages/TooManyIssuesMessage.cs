using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message describing that too many issues were found.
    /// </summary>
    [Serializable]
    public class TooManyIssuesMessage : IssueMessageBase
    {
        public int MaxIssueCount { get; }

        public TooManyIssuesMessage(int maxIssueCount, IssueKind issueKind)
            : base(IssueType.TooManyIssues, issueKind)
        {
            MaxIssueCount = maxIssueCount;
        }

        public override string ToString()
            => $"{IssueDefinitions.TooManyIssuesIssue.StaticDescription} ({MaxIssueCount})";
    }
}