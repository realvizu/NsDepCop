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
        public Parsers Parser { get; }

        public bool ChildCanDependOnParentImplicitly { get; }
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules { get; }
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int MaxIssueCount { get; }

        public AnalyzerConfig(
            bool isEnabled,
            IssueKind issueKind,
            Importance infoImportance,
            Parsers parser,
            bool childCanDependOnParentImplicitly,
            ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> allowRules,
            ImmutableHashSet<NamespaceDependencyRule> disallowRules,
            ImmutableDictionary<Namespace, TypeNameSet> visibleTypesByNamespace,
            int maxIssueCount)
        {
            IsEnabled = isEnabled;
            IssueKind = issueKind;
            InfoImportance = infoImportance;
            Parser = parser;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            MaxIssueCount = maxIssueCount;
        }

        public IEnumerable<string> DumpToStrings()
        {
            yield return $"IsEnabled={IsEnabled}";
            yield return $"IssueKind={IssueKind}";
            yield return $"InfoImportance={InfoImportance}";
            yield return $"Parser={Parser}";

            yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            foreach (var s in DumpAllowRulesToStrings()) yield return s;
            foreach (var s in DumpDisallowRulesToStrings()) yield return s;
            foreach (var s in DumpVisibleTypesByNamespaceToStrings()) yield return s;
            yield return $"MaxIssueCount={MaxIssueCount}";
        }

        private IEnumerable<string> DumpAllowRulesToStrings()
        {
            yield return $"AllowRules={AllowRules.Count}";
            foreach (var allowRule in AllowRules)
            {
                yield return $"  {allowRule.Key}";
                if (allowRule.Value != null)
                    foreach (var typeName in allowRule.Value)
                        yield return $"    {typeName}";
            }
        }

        private IEnumerable<string> DumpDisallowRulesToStrings()
        {
            yield return $"DisallowRules={DisallowRules.Count}";
            foreach (var disallowRule in DisallowRules)
                yield return $"  {disallowRule}";
        }

        private IEnumerable<string> DumpVisibleTypesByNamespaceToStrings()
        {
            yield return $"VisibleTypesByNamespace={VisibleTypesByNamespace.Count}";
            foreach (var visibleTypesByNamespace in VisibleTypesByNamespace)
            {
                yield return $"  {visibleTypesByNamespace.Key}";
                if (visibleTypesByNamespace.Value != null)
                    foreach (var typeName in visibleTypesByNamespace.Value)
                        yield return $"    {typeName}";
            }
        }
    }
}