using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Analyzes dependencies in source code based on a config.
    /// </summary>
    public interface IDependencyAnalyzer : IConfigProvider, ICacheStatisticsProvider, IDisposable
    {
        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns illegal dependencies (according to the rules described in the config).
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of illegal dependencies. Empty collection if none found.</returns>
        IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);

        /// <summary>
        /// Analyzes a syntax node and returns illegal dependencies (according to the rules described in the config).
        /// </summary>
        /// <param name="syntaxNode">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the project being analyzed.</param>
        /// <returns>A collection of illegal dependencies. Empty collection if none found.</returns>
        IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel);
    }
}