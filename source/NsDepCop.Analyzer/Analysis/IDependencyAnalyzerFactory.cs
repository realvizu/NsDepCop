using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Analysis
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        IDependencyAnalyzer Create(IUpdateableConfigProvider configProvider, ITypeDependencyEnumerator typeDependencyEnumerator);
    }
}