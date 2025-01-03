namespace Codartis.NsDepCop.Analysis.Implementation
{
    public interface IAssemblyDependencyValidator
    {
        DependencyStatus IsDependencyAllowed(AssemblyDependency assemblyDependency);
    }
}