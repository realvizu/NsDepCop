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
        public Dictionary<DependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<DependencyRule> DisallowRules { get; }
        public Dictionary<Domain, TypeNameSet> VisibleTypesByNamespace { get; }
        public HashSet<DependencyRule> AllowedAssemblyRules { get; }
        public HashSet<DependencyRule> DisallowedAssemblyRules { get; }
        public int MaxIssueCount { get; }
        public bool AutoLowerMaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled,
            string[] sourcePathExclusionPatterns,
            bool checkAssemblyDependencies,
            bool childCanDependOnParentImplicitly,
            bool parentCanDependOnChildImplicitly,
            Dictionary<DependencyRule, TypeNameSet> allowRules,
            HashSet<DependencyRule> disallowRules,
            Dictionary<Domain, TypeNameSet> visibleTypesByNamespace,
            HashSet<DependencyRule> allowedAssemblyRules,
            HashSet<DependencyRule> disallowedAssemblyRules,
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
            AllowedAssemblyRules = allowedAssemblyRules;
            DisallowedAssemblyRules = disallowedAssemblyRules;
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
            foreach (var s in AllowedAssemblyRules.ToStrings()) yield return s;
            foreach (var s in DisallowedAssemblyRules.ToStrings()) yield return s;
            foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
        }
    }
}