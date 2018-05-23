using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    [Serializable]
    public class IllegalDependencyMessage : IssueMessageBase
    {
        public TypeDependency IllegalDependency { get; }

        public IllegalDependencyMessage(TypeDependency illegalDependency, IssueKind issueKind)
            : base(IssueType.IllegalDependency, issueKind)
        {
            IllegalDependency = illegalDependency;
        }

        public override string ToString()
            => IssueDefinitions.IllegalDependencyIssue.DescriptionFormatterDelegate.Invoke(IllegalDependency);
    }
}