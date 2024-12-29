using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Analysis
{
    public interface IAssemblyDependencyAnalyzerFactory
    {
        IAssemblyDependencyAnalyzer Create(IUpdateableConfigProvider configProvider);
    }
}