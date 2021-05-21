using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        IDependencyAnalyzer Create(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator);
    }
}