using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Describes the config for a dependency analyzer. Immutable.
    /// </summary>
    internal class AnalyzerConfig : IAnalyzerConfig
    {
        public bool IsEnabled { get; }
        public IssueKind IssueKind { get; }
        public Importance InfoImportance { get; }

        public bool ChildCanDependOnParentImplicitly { get; }
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules { get; }
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int MaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled,
            IssueKind issueKind,
            Importance infoImportance,
            bool childCanDependOnParentImplicitly,
            ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> allowRules,
            ImmutableHashSet<NamespaceDependencyRule> disallowRules,
            ImmutableDictionary<Namespace, TypeNameSet> visibleTypesByNamespace,
            int maxIssueCount)
        {
            IsEnabled = isEnabled;
            IssueKind = issueKind;
            InfoImportance = infoImportance;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            MaxIssueCount = maxIssueCount;
        }

        public IEnumerable<string> ToStrings()
        {
            yield return $"IsEnabled={IsEnabled}";
            yield return $"IssueKind={IssueKind}";
            yield return $"InfoImportance={InfoImportance}";

            yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            foreach (var s in AllowRules.ToStrings()) yield return s;
            foreach (var s in DisallowRules.ToStrings()) yield return s;
            foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
        }
    }
}