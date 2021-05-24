namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message containing an illegal type dependency.
    /// </summary>
    public sealed class IllegalDependencyMessage : AnalyzerMessageBase
    {
        public TypeDependency IllegalDependency { get; }

        public IllegalDependencyMessage(TypeDependency illegalDependency)
        {
            IllegalDependency = illegalDependency;
        }
    }
}