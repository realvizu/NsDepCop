using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Describes the config for a dependency analyzer. Immutable.
    /// </summary>
    [Serializable]
    internal class AnalyzerConfig : IAnalyzerConfig
    {
        public bool IsEnabled { get; }
        public string[] SourcePathExclusionPatterns { get; }
        public bool ChildCanDependOnParentImplicitly { get; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public Dictionary<NamespaceDependencyRule, RuleLocation> RuleLocations { get; }
        public int MaxIssueCount { get; }
        public bool AutoLowerMaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled,
            string[] sourcePathExclusionPatterns,
            bool childCanDependOnParentImplicitly,
            Dictionary<NamespaceDependencyRule, TypeNameSet> allowRules,
            HashSet<NamespaceDependencyRule> disallowRules,
            Dictionary<Namespace, TypeNameSet> visibleTypesByNamespace,
            Dictionary<NamespaceDependencyRule, RuleLocation> ruleLocations,
            int maxIssueCount,
            bool autoLowerMaxIssueCount)
        {
            IsEnabled = isEnabled;
            SourcePathExclusionPatterns = sourcePathExclusionPatterns;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            RuleLocations = ruleLocations;
            MaxIssueCount = maxIssueCount;
            AutoLowerMaxIssueCount = autoLowerMaxIssueCount;
        }

        public IEnumerable<string> ToStrings()
        {
            yield return $"IsEnabled={IsEnabled}";
            yield return $"SourcePathExclusionPatterns={string.Join(",", SourcePathExclusionPatterns)}";
            yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            foreach (var s in AllowRules.ToStrings()) yield return s;
            foreach (var s in DisallowRules.ToStrings()) yield return s;
            foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            foreach (var s in RuleLocations.ToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
        }

        public RuleLocation GetRuleLocation(NamespaceDependencyRule namespaceDependencyRule)
        {
            return RuleLocations.TryGetValue(namespaceDependencyRule, out var ruleLocation) ? ruleLocation : null;
        }
    }
}