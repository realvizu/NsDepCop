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
        public bool CheckAssemblyDependencies { get; }
        public bool ChildCanDependOnParentImplicitly { get; }
        public bool ParentCanDependOnChildImplicitly { get; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public HashSet<NamespaceDependencyRule> AllowAssemblyRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowAssemblyRules { get; }
        public int MaxIssueCount { get; }
        public bool AutoLowerMaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled, 
            string[] sourcePathExclusionPatterns,
            bool checkAssemblyDependencies,
            bool childCanDependOnParentImplicitly,
            bool parentCanDependOnChildImplicitly,
            Dictionary<NamespaceDependencyRule, TypeNameSet> allowRules, 
            HashSet<NamespaceDependencyRule> disallowRules,
            Dictionary<Namespace, TypeNameSet> visibleTypesByNamespace,
            HashSet<NamespaceDependencyRule> allowAssemblyRules,
            HashSet<NamespaceDependencyRule> disallowAssemblyRules,
            int maxIssueCount,
            bool autoLowerMaxIssueCount)
        {
            IsEnabled = isEnabled;
            SourcePathExclusionPatterns = sourcePathExclusionPatterns;
            CheckAssemblyDependencies = checkAssemblyDependencies;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            ParentCanDependOnChildImplicitly = parentCanDependOnChildImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            AllowAssemblyRules = allowAssemblyRules;
            DisallowAssemblyRules = disallowAssemblyRules;
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
            foreach (var s in AllowAssemblyRules.ToStrings()) yield return s;
            foreach (var s in DisallowAssemblyRules.ToStrings()) yield return s;
            foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
        }
    }
}