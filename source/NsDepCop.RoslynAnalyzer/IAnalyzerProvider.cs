using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    /// <summary>
    /// Provides configured dependency analyzer instances.
    /// </summary>
    public interface IAnalyzerProvider
    {
        /// <summary>
        /// Retrieves an up-to-date analyzer for a C# project file (csproj).
        /// </summary>
        /// <param name="csprojFilePath">The full path of a C# project file.</param>
        /// <returns>A dependency analyzer, or null if cannot be retrieved.</returns>
        IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath);
    }
}