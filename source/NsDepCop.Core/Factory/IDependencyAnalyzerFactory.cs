using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        IDependencyAnalyzer CreateInProcess(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator);

        IDependencyAnalyzer CreateOutOfProcess(string folderPath, string serviceAddress);
    }
}