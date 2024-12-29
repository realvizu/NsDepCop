using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis
{
    /// <summary>
    /// Performs project dependency analysis on a project or a syntax node.
    /// </summary>
    public interface IAssemblyDependencyAnalyzer
    {
        IEnumerable<AnalyzerMessageBase> AnalyzeProject(AssemblyIdentity sourceAssembly, IReadOnlyList<AssemblyIdentity> referencedAssemblies);

        /// <summary>
        /// Re-reads the config.
        /// </summary>
        void RefreshConfig();

        /// <summary>
        /// Gets the current config state.
        /// </summary>
        AnalyzerConfigState ConfigState { get; }

        /// <summary>
        /// Gets the config exception or null if there was no exception.
        /// </summary>
        Exception ConfigException { get; }

        /// <summary>
        /// Gets the current analyzer config or null if there was an error.
        /// </summary>
        IAnalyzerConfig Config { get; }
    }
}