using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// Service interface for a remote (out-of-process) dependency analyzer.
    /// </summary>
    /// <remarks>
    /// Uses remotable types and stateless operations.
    /// </remarks>
    public interface IRemoteDependencyAnalyzer
    {
        /// <summary>
        /// Performs dependency analysis for a project.
        /// </summary>
        /// <param name="config">The config for the project.</param>
        /// <param name="sourcePaths">Source file names with full path.</param>
        /// <param name="referencedAssemblyPaths">Referenced assembly filenames with full path.</param>
        /// <returns>All messages emitted by the analyzer: dependency validations and trace messages.</returns>
        IRemoteMessage[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths);
    }
}
