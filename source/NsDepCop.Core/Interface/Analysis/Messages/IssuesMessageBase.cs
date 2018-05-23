using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// Abstract base class for messages that describe an issue.
    /// </summary>
    [Serializable]
    public abstract class IssueMessageBase : AnalyzerMessageBase
    {
        public IssueType IssueType { get; }
        public IssueKind IssueKind { get; }

        protected IssueMessageBase(IssueType issueType, IssueKind issueKind)
        {
            IssueType = issueType;
            IssueKind = issueKind;
        }
    }
}