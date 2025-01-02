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
        public int? InheritanceDepth { get; private set; }
        public bool? IsEnabled { get; private set; }
        public List<string> SourcePathExclusionPatterns { get; private set; }
        public bool? CheckAssemblyDependencies { get; private set; }
        public bool? ChildCanDependOnParentImplicitly { get; private set; }
        public bool? ParentCanDependOnChildImplicitly { get; private set; }
        public Dictionary<DependencyRule, TypeNameSet> AllowRules { get; }
        public HashSet<DependencyRule> DisallowRules { get; }
        public Dictionary<Domain, TypeNameSet> VisibleTypesByNamespace { get; }
        public HashSet<DependencyRule> AllowedAssemblyRules { get; }
        public HashSet<DependencyRule> DisallowedAssemblyRules { get; }
        public int? MaxIssueCount { get; private set; }
        public bool? AutoLowerMaxIssueCount { get; private set; }

        public AnalyzerConfigBuilder()
        {
            SourcePathExclusionPatterns = new List<string>();
            AllowRules = new Dictionary<DependencyRule, TypeNameSet>();
            DisallowRules = new HashSet<DependencyRule>();
            VisibleTypesByNamespace = new Dictionary<Domain, TypeNameSet>();
            AllowedAssemblyRules = new HashSet<DependencyRule>();
            DisallowedAssemblyRules = new HashSet<DependencyRule>();
        }

        public IAnalyzerConfig ToAnalyzerConfig()
        {
            return new AnalyzerConfig(
                IsEnabled ?? ConfigDefaults.IsEnabled,
                SourcePathExclusionPatterns.ToArray(),
                CheckAssemblyDependencies ?? ConfigDefaults.CheckAssemblyDependencies,
                ChildCanDependOnParentImplicitly ?? ConfigDefaults.ChildCanDependOnParentImplicitly,
                ParentCanDependOnChildImplicitly ?? ConfigDefaults.ParentCanDependOnChildImplicitly,
                AllowRules,
                DisallowRules,
                VisibleTypesByNamespace,
                AllowedAssemblyRules,
                DisallowedAssemblyRules,
                MaxIssueCount ?? ConfigDefaults.MaxIssueCount,
                AutoLowerMaxIssueCount ?? ConfigDefaults.AutoLowerMaxIssueCount
            );
        }

        public AnalyzerConfigBuilder Combine(AnalyzerConfigBuilder analyzerConfigBuilder)
        {
            // Note that InheritanceDepth is not combined.

            SetIsEnabled(analyzerConfigBuilder.IsEnabled);
            AddSourcePathExclusionPatterns(analyzerConfigBuilder.SourcePathExclusionPatterns);
            SetCheckAssemblyDependencies(analyzerConfigBuilder.CheckAssemblyDependencies);
            SetChildCanDependOnParentImplicitly(analyzerConfigBuilder.ChildCanDependOnParentImplicitly);
            SetParentCanDependOnChildImplicitly(analyzerConfigBuilder.ParentCanDependOnChildImplicitly);
            AddAllowRules(analyzerConfigBuilder.AllowRules);
            AddDisallowRules(analyzerConfigBuilder.DisallowRules);
            AddVisibleTypesByNamespace(analyzerConfigBuilder.VisibleTypesByNamespace);
            AddAllowedAssemblyRules(analyzerConfigBuilder.AllowedAssemblyRules);
            AddDisallowedAssemblyRules(analyzerConfigBuilder.DisallowedAssemblyRules);
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

        public AnalyzerConfigBuilder SetCheckAssemblyDependencies(bool? checkAssemblyDependencies)
        {
            if (checkAssemblyDependencies.HasValue)
                CheckAssemblyDependencies = checkAssemblyDependencies;
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

        public AnalyzerConfigBuilder SetParentCanDependOnChildImplicitly(bool? parentCanDependOnChildImplicitly)
        {
            if (parentCanDependOnChildImplicitly.HasValue)
                ParentCanDependOnChildImplicitly = parentCanDependOnChildImplicitly;
            return this;
        }

        public AnalyzerConfigBuilder AddAllowRule(DependencyRule dependencyRule, TypeNameSet typeNameSet = null)
        {
            AllowRules.AddOrUnion<DependencyRule, TypeNameSet, string>(dependencyRule, typeNameSet);
            return this;
        }

        private AnalyzerConfigBuilder AddAllowRules(IEnumerable<KeyValuePair<DependencyRule, TypeNameSet>> allowRules)
        {
            foreach (var keyValuePair in allowRules)
                AddAllowRule(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public AnalyzerConfigBuilder AddDisallowRule(DependencyRule dependencyRule)
        {
            DisallowRules.Add(dependencyRule);
            return this;
        }

        private AnalyzerConfigBuilder AddDisallowRules(IEnumerable<DependencyRule> disallowRules)
        {
            foreach (var dependencyRule in disallowRules)
                AddDisallowRule(dependencyRule);
            return this;
        }

        public AnalyzerConfigBuilder AddAllowedAssemblyRule(DependencyRule assemblyDependencyRule)
        {
            AllowedAssemblyRules.Add(assemblyDependencyRule);
            return this;
        }

        private AnalyzerConfigBuilder AddAllowedAssemblyRules(IEnumerable<DependencyRule> assemblyDependencyRules)
        {
            foreach (var assemblyDependencyRule in assemblyDependencyRules)
                AddAllowedAssemblyRule(assemblyDependencyRule);
            return this;
        }

        public AnalyzerConfigBuilder AddDisallowedAssemblyRule(DependencyRule assemblyDependencyRule)
        {
            DisallowedAssemblyRules.Add(assemblyDependencyRule);
            return this;
        }

        private AnalyzerConfigBuilder AddDisallowedAssemblyRules(IEnumerable<DependencyRule> assemblyDependencyRules)
        {
            foreach (var assemblyDependencyRule in assemblyDependencyRules)
                AddDisallowedAssemblyRule(assemblyDependencyRule);
            return this;
        }

        public AnalyzerConfigBuilder AddVisibleTypesByNamespace(Domain ns, TypeNameSet typeNameSet)
        {
            VisibleTypesByNamespace.AddOrUnion<Domain, TypeNameSet, string>(ns, typeNameSet);
            return this;
        }

        private AnalyzerConfigBuilder AddVisibleTypesByNamespace(IEnumerable<KeyValuePair<Domain, TypeNameSet>> visibleTypesByNamespace)
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
            if (CheckAssemblyDependencies.HasValue) yield return $"CheckAssemblyDependencies={CheckAssemblyDependencies}";
            if (ChildCanDependOnParentImplicitly.HasValue) yield return $"ChildCanDependOnParentImplicitly={ChildCanDependOnParentImplicitly}";
            if (ParentCanDependOnChildImplicitly.HasValue) yield return $"ParentCanDependOnChildImplicitly={ParentCanDependOnChildImplicitly}";
            if (AllowRules.Any())
                foreach (var s in AllowRules.ToStrings())
                    yield return s;
            if (DisallowRules.Any())
                foreach (var s in DisallowRules.ToStrings())
                    yield return s;
            if (AllowedAssemblyRules.Any())
                foreach (var s in AllowedAssemblyRules.ToStrings())
                    yield return s;
            if (DisallowedAssemblyRules.Any())
                foreach (var s in DisallowedAssemblyRules.ToStrings())
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