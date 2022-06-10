#nullable enable

using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis;

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
    /// Gets the current config state.
    /// </summary>
    AnalyzerConfigState ConfigState { get; }

    /// <summary>
    /// Gets the config exception or null if there was no exception.
    /// </summary>
    Exception? ConfigException { get; }

    /// <summary>
    /// Gets the current analyzer config or null if there was an error.
    /// </summary>
    IAnalyzerConfig Config { get; }

    /// <summary>
    /// Resets the state used for tracking which rules were actually used during the analysis of a compilation.
    /// </summary>
    void ResetRuleUsageTracking();

    /// <summary>
    /// Return those allow rules that were not used since the last call to <see cref="ResetRuleUsageTracking"/>.
    /// </summary>
    IReadOnlyCollection<NamespaceDependencyRule> GetUnusedAllowRules();

    /// <summary>
    /// Returns the location of a rule or null if unknown.
    /// Used for error reporting.
    /// </summary>
    RuleLocation? GetRuleLocation(NamespaceDependencyRule namespaceDependencyRule);
}