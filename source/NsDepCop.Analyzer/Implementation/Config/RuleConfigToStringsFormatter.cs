using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Static helper class for formatting rule config data into strings.
    /// </summary>
    public static class RuleConfigToStringsFormatter
    {
        public static IEnumerable<string> ToStrings(this IDictionary<NamespaceDependencyRule, TypeNameSet> allowRules)
        {
            yield return $"AllowRules={allowRules.Count}";
            foreach (var allowRule in allowRules)
                yield return $"  {allowRule.Key}, {allowRule.Value?.ToString() ?? "{}"}";
        }

        public static IEnumerable<string> ToStrings(this ISet<NamespaceDependencyRule> disallowRules)
        {
            yield return $"DisallowRules={disallowRules.Count}";
            foreach (var disallowRule in disallowRules)
                yield return $"  {disallowRule}";
        }

        public static IEnumerable<string> ToStrings(this IDictionary<Namespace, TypeNameSet> visibleTypesByNamespaces)
        {
            yield return $"VisibleTypesByNamespace={visibleTypesByNamespaces.Count}";
            foreach (var visibleTypesByNamespace in visibleTypesByNamespaces)
                yield return $"  {visibleTypesByNamespace.Key}, {visibleTypesByNamespace.Value?.ToString() ?? "{}"}";
        }
    }
}
