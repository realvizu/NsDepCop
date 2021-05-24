using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// Abstract base class for messages that describe an issue.
    /// </summary>
    public abstract class IssueMessageBase : AnalyzerMessageBase
    {
        public abstract IssueDescriptor IssueDefinition { get; }

        public virtual string Code => IssueDefinition.Id;
        public virtual string Title => IssueDefinition.Title;
        public virtual IssueKind IssueKind => IssueDefinition.DefaultKind;
        public virtual SourceSegment? SourceSegment => null;
        public override string ToString() => Title;
    }
}