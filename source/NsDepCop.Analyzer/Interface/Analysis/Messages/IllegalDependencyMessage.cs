using Codartis.NsDepCop.Interface.Config;

namespace Codartis.NsDepCop.Interface.Analysis.Messages
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    public sealed class IllegalDependencyMessage : IssueMessageBase
    {
        public override IssueDescriptor IssueDefinition => IssueDefinitions.IllegalDependencyIssue;

        public TypeDependency IllegalDependency { get; }
        public override IssueKind IssueKind { get; }

        public IllegalDependencyMessage(TypeDependency illegalDependency, IssueKind issueKind)
        {
            IllegalDependency = illegalDependency;
            IssueKind = issueKind;
        }

        public override SourceSegment? SourceSegment => IllegalDependency.SourceSegment;

        public override string ToString()
        {
            var i = IllegalDependency;
            return $"Illegal namespace reference: {i.FromNamespaceName}->{i.ToNamespaceName} (Type: {i.FromTypeName}->{i.ToTypeName})";
        }
    }
}