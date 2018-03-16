using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    [Serializable]
    public class IllegalDependencyMessage : AnalyzerMessageBase
    {
        public TypeDependency IllegalDependency { get; }

        public IllegalDependencyMessage(TypeDependency illegalDependency)
        {
            IllegalDependency = illegalDependency;
        }
    }
}
