using System;

namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    public sealed class IllegalDependencyMessage : AnalyzerMessageBase
    {
        public TypeDependency IllegalDependency { get; }

        public string[] AllowedMemberNames { get; } = Array.Empty<string>();

        public IllegalDependencyMessage(TypeDependency illegalDependency, string[] allowedMemberNames)
        {
            IllegalDependency = illegalDependency;
            AllowedMemberNames = allowedMemberNames;
        }
    }
}