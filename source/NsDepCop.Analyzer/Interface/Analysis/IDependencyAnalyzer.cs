using System.Collections.Generic;
using Codartis.NsDepCop.Interface.Analysis.Messages;
using Codartis.NsDepCop.Interface.Config;

namespace Codartis.NsDepCop.Interface.Analysis
{
    /// <summary>
    /// Performs dependency analysis on a project or a syntax node.
    /// </summary>
    public interface IDependencyAnalyzer
    {
        /// <summary>
        /// Analyzes a project (source files and referenced assemblies).
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>Issue and info messages, including illegal dependency issues.</returns>
        IEnumerable<AnalyzerMessageBase> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);

        /// <summary>
        /// Analyzes a syntax node.
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project being analyzed.</param>
        /// <returns>Issue and info messages, including illegal dependency issues.</returns>
        IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel);

        /// <summary>
        /// Gets the importance of the info messages.
        /// </summary>
        Importance InfoImportance { get; }

        /// <summary>
        /// Re-reads the config.
        /// </summary>
        void RefreshConfig();
    }
}