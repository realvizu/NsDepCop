using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Builds analyzer config objects.
    /// </summary>
    public class AnalyzerConfigBuilder
    {
        private readonly string _configFilePath;
        private readonly ConfigFileScope? _configFileScope;

        public int? InheritanceDepth { get; private set; }
        public bool? IsEnabled { get; private set; }
        public List<string> SourcePathExclusionPatterns { get; private set; }
        public bool? ChildCanDependOnParentImplicitly { get; private set; }
        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<NamespaceDependencyRule> DisallowRules { get; }
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public Dictionary<NamespaceDependencyRule, RuleLocation> RuleLocations { get; }
        public int? MaxIssueCount { get; private set; }
        public bool? AutoLowerMaxIssueCount { get; private set; }

        public AnalyzerConfigBuilder(
            string configFilePath = null,
            ConfigFileScope? configFileScope = null)
        {
            _configFilePath = configFilePath;
            _configFileScope = configFileScope;
            SourcePathExclusionPatterns = new List<string>();
            AllowRules = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            DisallowRules = new HashSet<NamespaceDependencyRule>();
            VisibleTypesByNamespace = new Dictionary<Namespace, TypeNameSet>();
            RuleLocations = new Dictionary<NamespaceDependencyRule, RuleLocation>();
        }

        public IAnalyzerConfig ToAnalyzerConfig()
        {
            return new AnalyzerConfig(
                IsEnabled ?? ConfigDefaults.IsEnabled,
                SourcePathExclusionPatterns.ToArray(),
                ChildCanDependOnParentImplicitly ?? ConfigDefaults.ChildCanDependOnParentImplicitly,
                AllowRules,
                DisallowRules,
                VisibleTypesByNamespace,
                RuleLocations,
                MaxIssueCount ?? ConfigDefaults.MaxIssueCount,
                AutoLowerMaxIssueCount ?? ConfigDefaults.AutoLowerMaxIssueCount
            );
        }

        public AnalyzerConfigBuilder Combine(AnalyzerConfigBuilder analyzerConfigBuilder)
        {
            // Note that InheritanceDepth is not combined.

            SetIsEnabled(analyzerConfigBuilder.IsEnabled);
            AddSourcePathExclusionPatterns(analyzerConfigBuilder.SourcePathExclusionPatterns);

            SetChildCanDependOnParentImplicitly(analyzerConfigBuilder.ChildCanDependOnParentImplicitly);
            AddAllowRules(analyzerConfigBuilder.AllowRules);
            AddDisallowRules(analyzerConfigBuilder.DisallowRules);
            AddVisibleTypesByNamespace(analyzerConfigBuilder.VisibleTypesByNamespace);
            AddRuleLocations(analyzerConfigBuilder.RuleLocations);
            SetMaxIssueCount(analyzerConfigBuilder.MaxIssueCount);
            SetAutoLowerMaxIssueCount(analyzerConfigBuilder.AutoLowerMaxIssueCount);

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

        public AnalyzerConfigBuilder AddSourcePathExclusionPatterns(IEnumerable<string> sourcePathExclusionPatterns)
        {
            if (sourcePathExclusionPatterns != null)
                SourcePathExclusionPatterns.AddRange(sourcePathExclusionPatterns);

            return this;
        }

        public AnalyzerConfigBuilder MakePathsRooted(string rootPath)
        {
            if (!Path.IsPathRooted(rootPath))
                throw new ArgumentException($"Rooted path expected: {rootPath}");

            SourcePathExclusionPatterns = SourcePathExclusionPatterns.Select(i => ToRootedPath(rootPath, i)).ToList();

            return this;
        }

        public AnalyzerConfigBuilder SetChildCanDependOnParentImplicitly(bool? childCanDependOnParentImplicitly)
        {
            if (childCanDependOnParentImplicitly.HasValue)
                ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            return this;
        }

        public AnalyzerConfigBuilder AddAllowRule(
            NamespaceDependencyRule namespaceDependencyRule,
            TypeNameSet typeNameSet = null,
            int? lineNumber = null,
            int? linePosition = null)
        {
            AllowRules.AddOrUnion<NamespaceDependencyRule, TypeNameSet, string>(namespaceDependencyRule, typeNameSet);

            var ruleLocation = GetRuleLocation(lineNumber, linePosition);
            if (ruleLocation != null) AddRuleLocation(namespaceDependencyRule, ruleLocation);

            return this;
        }

        private void AddRuleLocation(NamespaceDependencyRule namespaceDependencyRule, RuleLocation ruleLocation)
        {
            if (!RuleLocations.ContainsKey(namespaceDependencyRule))
                RuleLocations.Add(namespaceDependencyRule, ruleLocation);
        }

        private RuleLocation GetRuleLocation(int? lineNumber, int? linePosition)
        {
            return _configFilePath != null && _configFileScope != null && lineNumber != null && linePosition != null
                ? new RuleLocation(_configFilePath, _configFileScope.Value, lineNumber.Value, linePosition.Value)
                : null;
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

        private AnalyzerConfigBuilder AddRuleLocations(IEnumerable<KeyValuePair<NamespaceDependencyRule, RuleLocation>> ruleLocations)
        {
            foreach (var keyValuePair in ruleLocations)
                AddRuleLocation(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public AnalyzerConfigBuilder SetMaxIssueCount(int? maxIssueCount)
        {
            if (maxIssueCount.HasValue)
                MaxIssueCount = maxIssueCount;
            return this;
        }

        public AnalyzerConfigBuilder SetAutoLowerMaxIssueCount(bool? autoLowerMaxIssueCount)
        {
            if (autoLowerMaxIssueCount.HasValue)
                AutoLowerMaxIssueCount = autoLowerMaxIssueCount;
            return this;
        }

        public IEnumerable<string> ToStrings()
        {
            if (InheritanceDepth.HasValue) yield return $"InheritanceDepth={InheritanceDepth}";
            if (IsEnabled.HasValue) yield return $"IsEnabled={IsEnabled}";
            if (SourcePathExclusionPatterns != null) yield return $"SourcePathExclusionPatterns={string.Join(";", SourcePathExclusionPatterns)}";
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
        }

        private static string ToRootedPath(string rootPath, string path)
        {
            if (rootPath == null || Path.IsPathRooted(path))
                return path;

            return Path.Combine(rootPath, path);
        }
    }
}