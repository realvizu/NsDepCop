using System;

namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    public sealed class IllegalDependencyMessage : AnalyzerMessageBase
    {
        public TypeDependency IllegalDependency { get; }

        public string[] AllowedTypeNames { get; } = Array.Empty<string>();

        public IllegalDependencyMessage(TypeDependency illegalDependency, string[] allowedTypeNames)
        {
            IllegalDependency = illegalDependency;
        }

        // public IllegalDependencyMessage(TypeDependency illegalDependency, string[] allowedTypeNames)
        // {
        //     IllegalDependency = illegalDependency;
        //     AllowedTypeNames = allowedTypeNames;
        // }
    }
}