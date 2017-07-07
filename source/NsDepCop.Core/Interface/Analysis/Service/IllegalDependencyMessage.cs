using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Service
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    [Serializable]
    public class IllegalDependencyMessage
    {
        public TypeDependency IllegalDependency { get; }

        public IllegalDependencyMessage(TypeDependency illegalDependency)
        {
            IllegalDependency = illegalDependency;
        }
    }
}
