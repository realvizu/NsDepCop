namespace Codartis.NsDepCop.Analysis.Messages
{
    public sealed class IllegalAssemblyDependencyMessage : AnalyzerMessageBase
    {
        public AssemblyDependency IllegalAssemblyDependency { get; }

        public IllegalAssemblyDependencyMessage(AssemblyDependency illegalAssemblyDependency)
        {
            IllegalAssemblyDependency = illegalAssemblyDependency;
        }
    }
}