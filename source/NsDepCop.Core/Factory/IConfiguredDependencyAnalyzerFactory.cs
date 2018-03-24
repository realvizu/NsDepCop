using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Configured;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates objects that bundle together a dependency analyzer and its config provider.
    /// </summary>
    public interface IConfiguredDependencyAnalyzerFactory
    {
        IConfiguredDependencyAnalyzer CreateInProcess(string folderPath, ITypeDependencyEnumerator typeDependencyEnumerator);

        IConfiguredDependencyAnalyzer CreateOutOfProcess(string folderPath, string serviceAddress);
    }
}