using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Analysis.Factory
{
    public sealed class AssemblyDependencyAnalyzerFactory : IAssemblyDependencyAnalyzerFactory
    {
        public IAssemblyDependencyAnalyzer Create(IUpdateableConfigProvider configProvider)
        {

            return new AssemblyDependencyAnalyzer(configProvider);
        }
    }
}
