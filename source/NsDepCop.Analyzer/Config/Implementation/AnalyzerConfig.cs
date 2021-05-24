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
        public IssueKind DependencyIssueSeverity { get; }
        public Importance InfoImportance { get; }
        public TimeSpan[] AnalyzerServiceCallRetryTimeSpans { get; }
        public string[] SourcePathExclusionPatterns { get; }

        public bool ChildCanDependOnParentImplicitly { get; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int MaxIssueCount { get; }
        public IssueKind MaxIssueCountSeverity { get; }
        public bool AutoLowerMaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled, 
            IssueKind issueKind, 
            Importance infoImportance,
            TimeSpan[] analyzerServiceCallRetryTimeSpans,
            string[] sourcePathExclusionPatterns,

            bool childCanDependOnParentImplicitly,
            Dictionary<NamespaceDependencyRule, TypeNameSet> allowRules, 
            HashSet<NamespaceDependencyRule> disallowRules, 
            Dictionary<Namespace, TypeNameSet> visibleTypesByNamespace, 
            int maxIssueCount,
            IssueKind maxIssueCountSeverity,
            bool autoLowerMaxIssueCount)
        {
            IsEnabled = isEnabled;
            DependencyIssueSeverity = issueKind;
            InfoImportance = infoImportance;
            AnalyzerServiceCallRetryTimeSpans = analyzerServiceCallRetryTimeSpans;
            SourcePathExclusionPatterns = sourcePathExclusionPatterns;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            MaxIssueCount = maxIssueCount;
            MaxIssueCountSeverity = maxIssueCountSeverity;
            AutoLowerMaxIssueCount = autoLowerMaxIssueCount;
        }

        public IEnumerable<string> ToStrings()
        {
            yield return $"IsEnabled={IsEnabled}";
            yield return $"DependencyIssueSeverity={DependencyIssueSeverity}";
            yield return $"InfoImportance={InfoImportance}";
            yield return $"AnalyzerServiceCallRetryTimeSpans={string.Join(",", AnalyzerServiceCallRetryTimeSpans)}";
            yield return $"SourcePathExclusionPatterns={string.Join(",", SourcePathExclusionPatterns)}";

            yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            foreach (var s in AllowRules.ToStrings()) yield return s;
            foreach (var s in DisallowRules.ToStrings()) yield return s;
            foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
            yield return $"MaxIssueCountSeverity={MaxIssueCountSeverity}";
        }
    }
}