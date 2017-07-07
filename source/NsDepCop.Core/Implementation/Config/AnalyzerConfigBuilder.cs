using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Builds analyzer config objects.
    /// </summary>
    public class AnalyzerConfigBuilder : IConfigInitializer<AnalyzerConfigBuilder>, IDiagnosticSupport
    {
        public Importance? DefaultInfoImportance { get; private set; }

        public int? InheritanceDepth { get; private set; }

        public bool? IsEnabled { get; private set; }
        public IssueKind? IssueKind { get; private set; }
        public Importance? InfoImportance { get; private set; }

        public bool? ChildCanDependOnParentImplicitly { get; private set; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int? MaxIssueCount { get; private set; }

        public AnalyzerConfigBuilder()
        {
            AllowRules = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            DisallowRules = new HashSet<NamespaceDependencyRule>();
            VisibleTypesByNamespace = new Dictionary<Namespace, TypeNameSet>();
        }

        public IAnalyzerConfig ToAnalyzerConfig()
        {
            return new AnalyzerConfig(
                IsEnabled ?? ConfigDefaults.IsEnabled,
                IssueKind ?? ConfigDefaults.IssueKind,
                InfoImportance ?? DefaultInfoImportance ?? ConfigDefaults.InfoImportance,
                ChildCanDependOnParentImplicitly ?? ConfigDefaults.ChildCanDependOnParentImplicitly,
                AllowRules,
                DisallowRules,
                VisibleTypesByNamespace,
                MaxIssueCount ?? ConfigDefaults.MaxIssueCount
                );
        }

        public AnalyzerConfigBuilder Combine(AnalyzerConfigBuilder analyzerConfigBuilder)
        {
            // Note that InhertanceDepth is not combined.

            SetIsEnabled(analyzerConfigBuilder.IsEnabled);
            SetIssueKind(analyzerConfigBuilder.IssueKind);
            SetInfoImportance(analyzerConfigBuilder.InfoImportance);

            SetChildCanDependOnParentImplicitly(analyzerConfigBuilder.ChildCanDependOnParentImplicitly);
            AddAllowRules(analyzerConfigBuilder.AllowRules);
            AddDisallowRules(analyzerConfigBuilder.DisallowRules);
            AddVisibleTypesByNamespace(analyzerConfigBuilder.VisibleTypesByNamespace);
            SetMaxIssueCount(analyzerConfigBuilder.MaxIssueCount);

            return this;
        }

        public AnalyzerConfigBuilder SetDefaultInfoImportance(Importance? defaultInfoImportance)
        {
            DefaultInfoImportance = defaultInfoImportance;
            return this;
        }

        public AnalyzerConfigBuilder SetInheritanceDepth(int? inheritanceDepth)
        {
            if (inheritanceDepth.HasValue)
                InheritanceDepth = inheritanceDepth;
            return this;
        }

        public AnalyzerConfigBuilder SetIsEnabled(bool? isEnabled)
        {
            if (isEnabled.HasValue)
                IsEnabled = isEnabled;
            return this;
        }

        public AnalyzerConfigBuilder SetIssueKind(IssueKind? issueKind)
        {
            if (issueKind.HasValue)
                IssueKind = issueKind;
            return this;
        }

        public AnalyzerConfigBuilder SetInfoImportance(Importance? infoImportance)
        {
            if (infoImportance.HasValue)
                InfoImportance = infoImportance;
            return this;
        }

        public AnalyzerConfigBuilder SetChildCanDependOnParentImplicitly(bool? childCanDependOnParentImplicitly)
        {
            if (childCanDependOnParentImplicitly.HasValue)
                ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            return this;
        }

        public AnalyzerConfigBuilder AddAllowRule(NamespaceDependencyRule namespaceDependencyRule, TypeNameSet typeNameSet = null)
        {
            AllowRules.AddOrUnion<NamespaceDependencyRule, TypeNameSet, string>(namespaceDependencyRule, typeNameSet);
            return this;
        }

        private AnalyzerConfigBuilder AddAllowRules(IEnumerable<KeyValuePair<NamespaceDependencyRule, TypeNameSet>> allowRules)
        {
            foreach (var keyValuePair in allowRules)
                AddAllowRule(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public AnalyzerConfigBuilder AddDisallowRule(NamespaceDependencyRule namespaceDependencyRule)
        {
            DisallowRules.Add(namespaceDependencyRule);
            return this;
        }

        private AnalyzerConfigBuilder AddDisallowRules(IEnumerable<NamespaceDependencyRule> disallowRules)
        {
            foreach (var namespaceDependencyRule in disallowRules)
                AddDisallowRule(namespaceDependencyRule);
            return this;
        }

        public AnalyzerConfigBuilder AddVisibleTypesByNamespace(Namespace ns, TypeNameSet typeNameSet)
        {
            VisibleTypesByNamespace.AddOrUnion<Namespace, TypeNameSet, string>(ns, typeNameSet);
            return this;
        }

        private AnalyzerConfigBuilder AddVisibleTypesByNamespace(IEnumerable<KeyValuePair<Namespace, TypeNameSet>> visibleTypesByNamespace)
        {
            foreach (var keyValuePair in visibleTypesByNamespace)
                AddVisibleTypesByNamespace(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public AnalyzerConfigBuilder SetMaxIssueCount(int? maxIssueCount)
        {
            if (maxIssueCount.HasValue)
                MaxIssueCount = maxIssueCount;
            return this;
        }

        public IEnumerable<string> ToStrings()
        {
            if (InheritanceDepth.HasValue) yield return $"InheritanceDepth={InheritanceDepth}";
            if (IsEnabled.HasValue) yield return $"IsEnabled={IsEnabled}";
            if (IssueKind.HasValue) yield return $"IssueKind={IssueKind}";
            if (InfoImportance.HasValue) yield return $"InfoImportance={InfoImportance}";

            if (ChildCanDependOnParentImplicitly.HasValue) yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            if (AllowRules.Any()) foreach (var s in AllowRules.ToStrings()) yield return s;
            if (DisallowRules.Any()) foreach (var s in DisallowRules.ToStrings()) yield return s;
            if (VisibleTypesByNamespace.Any()) foreach (var s in VisibleTypesByNamespace.ToStrings()) yield return s;
            if (MaxIssueCount.HasValue) yield return $"MaxIssueCount={MaxIssueCount}";
        }
    }
}