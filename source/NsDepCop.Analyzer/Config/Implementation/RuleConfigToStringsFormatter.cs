using System.Collections.Generic;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Static helper class for formatting rule config data into strings.
    /// </summary>
    public static class RuleConfigToStringsFormatter
    {
        public static IEnumerable<string> ToStrings(this IDictionary<DependencyRule, TypeNameSet> allowRules)
        {
            yield return $"AllowRules={allowRules.Count}";
            foreach (var allowRule in allowRules)
                yield return $"  {allowRule.Key}, {allowRule.Value?.ToString() ?? "{}"}";
        }

        public static IEnumerable<string> ToStrings(this ISet<DependencyRule> disallowRules)
        {
            yield return $"DisallowRules={disallowRules.Count}";
            foreach (var disallowRule in disallowRules)
                yield return $"  {disallowRule}";
        }

        public static IEnumerable<string> ToStrings(this IDictionary<Domain, TypeNameSet> visibleTypesByNamespaces)
        {
            yield return $"VisibleTypesByNamespace={visibleTypesByNamespaces.Count}";
            foreach (var visibleTypesByNamespace in visibleTypesByNamespaces)
                yield return $"  {visibleTypesByNamespace.Key}, {visibleTypesByNamespace.Value?.ToString() ?? "{}"}";
        }
    }
}
