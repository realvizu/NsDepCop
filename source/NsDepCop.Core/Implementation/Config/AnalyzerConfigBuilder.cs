using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Builds analyzer config objects.
    /// </summary>
    internal class AnalyzerConfigBuilder : IConfigInitializer<AnalyzerConfigBuilder>, IDiagnosticSupport
    {
        public Importance? DefaultInfoImportance { get; private set; }

        public int? InheritanceDepth { get; private set; }

        public bool? IsEnabled { get; private set; }
        public IssueKind? DependencyIssueSeverity { get; private set; }
        public Importance? InfoImportance { get; private set; }
        public TimeSpan[] AnalyzerServiceCallRetryTimeSpans { get; private set; }
        public List<string> SourcePathExclusionPatterns { get; }
        public string RootPath { get; private set; }

        public bool? ChildCanDependOnParentImplicitly { get; private set; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int? MaxIssueCount { get; private set; }
        public IssueKind? MaxIssueCountSeverity { get; private set; }
        public bool? AutoLowerMaxIssueCount { get; private set; }

        public AnalyzerConfigBuilder()
        {
            SourcePathExclusionPatterns = new List<string>();
            AllowRules = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            DisallowRules = new HashSet<NamespaceDependencyRule>();
            VisibleTypesByNamespace = new Dictionary<Namespace, TypeNameSet>();
        }

        public IAnalyzerConfig ToAnalyzerConfig()
        {
            return new AnalyzerConfig(
                IsEnabled ?? ConfigDefaults.IsEnabled,
                DependencyIssueSeverity ?? ConfigDefaults.DependencyIssueSeverity,
                InfoImportance ?? DefaultInfoImportance ?? ConfigDefaults.InfoImportance,
                AnalyzerServiceCallRetryTimeSpans ?? ConfigDefaults.AnalyzerServiceCallRetryTimeSpans,
                SourcePathExclusionPatterns.Select(i => ToRootedPath(RootPath, i)).ToArray(),
                ChildCanDependOnParentImplicitly ?? ConfigDefaults.ChildCanDependOnParentImplicitly,
                AllowRules,
                DisallowRules,
                VisibleTypesByNamespace,
                MaxIssueCount ?? ConfigDefaults.MaxIssueCount,
                MaxIssueCountSeverity ?? ConfigDefaults.MaxIssueCountSeverity,
                AutoLowerMaxIssueCount ?? ConfigDefaults.AutoLowerMaxIssueCount
            );
        }

        public AnalyzerConfigBuilder Combine(AnalyzerConfigBuilder analyzerConfigBuilder)
        {
            // Note that InheritanceDepth is not combined.

            SetIsEnabled(analyzerConfigBuilder.IsEnabled);
            SetDependencyIssueSeverity(analyzerConfigBuilder.DependencyIssueSeverity);
            SetInfoImportance(analyzerConfigBuilder.InfoImportance);
            SetAnalyzerServiceCallRetryTimeSpans(analyzerConfigBuilder.AnalyzerServiceCallRetryTimeSpans);
            AddSourcePathExclusionPatterns(analyzerConfigBuilder.SourcePathExclusionPatterns);

            SetChildCanDependOnParentImplicitly(analyzerConfigBuilder.ChildCanDependOnParentImplicitly);
            AddAllowRules(analyzerConfigBuilder.AllowRules);
            AddDisallowRules(analyzerConfigBuilder.DisallowRules);
            AddVisibleTypesByNamespace(analyzerConfigBuilder.VisibleTypesByNamespace);
            SetMaxIssueCount(analyzerConfigBuilder.MaxIssueCount);
            SetMaxIssueCountSeverity(analyzerConfigBuilder.MaxIssueCountSeverity);
            SetAutoLowerMaxIssueCount(analyzerConfigBuilder.AutoLowerMaxIssueCount);

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

        public AnalyzerConfigBuilder SetDependencyIssueSeverity(IssueKind? dependencyIssueSeverity)
        {
            if (dependencyIssueSeverity.HasValue)
                DependencyIssueSeverity = dependencyIssueSeverity;
            return this;
        }

        public AnalyzerConfigBuilder SetInfoImportance(Importance? infoImportance)
        {
            if (infoImportance.HasValue)
                InfoImportance = infoImportance;
            return this;
        }

        public AnalyzerConfigBuilder SetAnalyzerServiceCallRetryTimeSpans(TimeSpan[] analyzerServiceCallRetryTimeSpans)
        {
            if (analyzerServiceCallRetryTimeSpans != null)
                AnalyzerServiceCallRetryTimeSpans = analyzerServiceCallRetryTimeSpans;
            return this;
        }

        public AnalyzerConfigBuilder AddSourcePathExclusionPatterns(IEnumerable<string> sourcePathExclusionPatterns)
        {
            if (sourcePathExclusionPatterns != null)
                SourcePathExclusionPatterns.AddRange(sourcePathExclusionPatterns);
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

        public AnalyzerConfigBuilder SetMaxIssueCountSeverity(IssueKind? maxIssueCountSeverity)
        {
            if (maxIssueCountSeverity.HasValue)
                MaxIssueCountSeverity = maxIssueCountSeverity;
            return this;
        }

        public AnalyzerConfigBuilder SetAutoLowerMaxIssueCount(bool? autoLowerMaxIssueCount)
        {
            if (autoLowerMaxIssueCount.HasValue)
                AutoLowerMaxIssueCount = autoLowerMaxIssueCount;
            return this;
        }

        public AnalyzerConfigBuilder SetRootPath(string rootPath)
        {
            if (rootPath != null)
            {
                if (!Path.IsPathRooted(rootPath))
                    throw new Exception($"Rooted path expected: {rootPath}");

                RootPath = rootPath;
            }

            return this;
        }

        public IEnumerable<string> ToStrings()
        {
            if (InheritanceDepth.HasValue) yield return $"InheritanceDepth={InheritanceDepth}";
            if (IsEnabled.HasValue) yield return $"IsEnabled={IsEnabled}";
            if (DependencyIssueSeverity.HasValue) yield return $"DependencyIssueSeverity={DependencyIssueSeverity}";
            if (InfoImportance.HasValue) yield return $"InfoImportance={InfoImportance}";
            if (AnalyzerServiceCallRetryTimeSpans != null) yield return $"AnalyzerServiceCallRetryTimeSpans={string.Join(";", AnalyzerServiceCallRetryTimeSpans)}";
            if (SourcePathExclusionPatterns != null) yield return $"SourcePathExclusionPatterns={string.Join(";", SourcePathExclusionPatterns)}";
            if (RootPath != null) yield return $"RootPath={RootPath}";

            if (ChildCanDependOnParentImplicitly.HasValue) yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            if (AllowRules.Any())
                foreach (var s in AllowRules.ToStrings())
                    yield return s;
            if (DisallowRules.Any())
                foreach (var s in DisallowRules.ToStrings())
                    yield return s;
            if (VisibleTypesByNamespace.Any())
                foreach (var s in VisibleTypesByNamespace.ToStrings())
                    yield return s;
            if (MaxIssueCount.HasValue) yield return $"MaxIssueCount={MaxIssueCount}";
            if (MaxIssueCountSeverity.HasValue) yield return $"MaxIssueCountSeverity={MaxIssueCountSeverity}";
        }

        private static string ToRootedPath(string rootPath, string path)
        {
            if (rootPath == null || Path.IsPathRooted(path))
                return path;

            return Path.Combine(rootPath, path);
        }
    }
}