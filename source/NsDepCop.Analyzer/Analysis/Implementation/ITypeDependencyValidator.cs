#nullable enable

using System.Collections.Generic;
using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Analysis.Implementation;

/// <summary>
/// Determines whether a type-to-type dependency is allowed or not.
/// </summary>
internal interface ITypeDependencyValidator
{
    /// <summary>
    /// Decides whether a dependency is allowed based on the rule configuration.
    /// </summary>
    /// <param name="typeDependency">A dependency of two types.</param>
    /// <returns>True if the dependency is allowed, false otherwise.</returns>
    bool IsAllowedDependency(TypeDependency typeDependency);

    /// <summary>
    /// Resets the state used for tracking which rules were actually used during the analysis of a compilation.
    /// </summary>
    void ResetRuleUsageTracking();

    /// <summary>
    /// Return those allow rules that were not used since the last call to <see cref="ResetRuleUsageTracking"/>.
    /// </summary>
    IReadOnlyCollection<NamespaceDependencyRule> GetUnusedAllowRules();
}