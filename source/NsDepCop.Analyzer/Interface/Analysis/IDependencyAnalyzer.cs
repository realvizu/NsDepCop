using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Interface.Analysis.Messages;
using Microsoft.CodeAnalysis;

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
        IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(SyntaxNode syntaxNode, SemanticModel semanticModel);

        /// <summary>
        /// Re-reads the config.
        /// </summary>
        void RefreshConfig();

        /// <summary>
        /// Returns true if the config reading produced an error.
        /// </summary>
        bool HasConfigError { get; }

        /// <summary>
        /// Returns true if the tool is disabled in config.
        /// </summary>
        bool IsDisabledInConfig { get; }

        /// <summary>
        /// Gets the config exception or null if there was no exception.
        /// </summary>
        Exception GetConfigException();

        int MaxIssueCount { get; }
    }
}