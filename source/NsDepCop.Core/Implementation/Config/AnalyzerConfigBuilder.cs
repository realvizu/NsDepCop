using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Builds analyzer config objects.
    /// </summary>
    internal class AnalyzerConfigBuilder
    {
        private readonly bool _isParserOverridden;

        private bool _isEnabled;
        private IssueKind _issueKind;
        private Importance _infoImportance;
        private Parsers _parser;

        private bool _childCanDependOnParentImplicitly;
        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowRules;
        private readonly HashSet<NamespaceDependencyRule> _disallowRules;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesByNamespace;
        private int _maxIssueCount;

        public AnalyzerConfigBuilder(Parsers? overridingParser = null)
        {
            _isParserOverridden = overridingParser.HasValue;

            _isEnabled = ConfigDefaults.IsEnabled;
            _issueKind = ConfigDefaults.IssueKind;
            _infoImportance = ConfigDefaults.InfoImportance;
            _parser = overridingParser ?? ConfigDefaults.Parser;

            _childCanDependOnParentImplicitly = ConfigDefaults.ChildCanDependOnParentImplicitly;
            _allowRules = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowRules = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByNamespace = new Dictionary<Namespace, TypeNameSet>();
            _maxIssueCount = ConfigDefaults.MaxIssueReported;
        }

        public AnalyzerConfigBuilder(IAnalyzerConfig analyzerConfig)
        {
            _isEnabled = analyzerConfig.IsEnabled;
            _issueKind = analyzerConfig.IssueKind;
            _infoImportance = analyzerConfig.InfoImportance;
            _parser = analyzerConfig.Parser;

            _childCanDependOnParentImplicitly = analyzerConfig.ChildCanDependOnParentImplicitly;
            _allowRules = analyzerConfig.AllowRules.ToDictionary(i => i.Key, i => i.Value);
            _disallowRules = analyzerConfig.DisallowRules.ToHashSet();
            _visibleTypesByNamespace = analyzerConfig.VisibleTypesByNamespace.ToDictionary(i => i.Key, i => i.Value);
            _maxIssueCount = analyzerConfig.MaxIssueCount;
        }

        public IAnalyzerConfig ToProjectConfig()
        {
            return new AnalyzerConfig(
                _isEnabled,
                _issueKind,
                _infoImportance,
                _parser,
                _childCanDependOnParentImplicitly,
                _allowRules.ToImmutableDictionary(),
                _disallowRules.ToImmutableHashSet(),
                _visibleTypesByNamespace.ToImmutableDictionary(),
                _maxIssueCount
                );
        }

        public AnalyzerConfigBuilder Combine(IAnalyzerConfig analyzerConfig)
        {
            SetIsEnabled(analyzerConfig.IsEnabled);
            SetIssueKind(analyzerConfig.IssueKind);
            SetInfoImportance(analyzerConfig.InfoImportance);
            SetParser(analyzerConfig.Parser);

            SetChildCanDependOnParentImplicitly(analyzerConfig.ChildCanDependOnParentImplicitly);
            AddAllowRules(analyzerConfig.AllowRules);
            AddDisallowRules(analyzerConfig.DisallowRules);
            AddVisibleTypesByNamespace(analyzerConfig.VisibleTypesByNamespace);
            SetMaxIssueCount(analyzerConfig.MaxIssueCount);

            return this;
        }

        public AnalyzerConfigBuilder SetIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            return this;
        }

        public AnalyzerConfigBuilder SetIssueKind(IssueKind issueKind)
        {
            _issueKind = issueKind;
            return this;
        }

        public AnalyzerConfigBuilder SetInfoImportance(Importance infoImportance)
        {
            _infoImportance = infoImportance;
            return this;
        }

        public AnalyzerConfigBuilder SetParser(Parsers parser)
        {
            if (!_isParserOverridden)
                _parser = parser;

            return this;
        }

        public AnalyzerConfigBuilder SetChildCanDependOnParentImplicitly(bool childCanDependOnParentImplicitly)
        {
            _childCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            return this;
        }

        public AnalyzerConfigBuilder AddAllowRule(NamespaceDependencyRule namespaceDependencyRule, TypeNameSet typeNameSet = null)
        {
            _allowRules.AddOrUnion<NamespaceDependencyRule, TypeNameSet, string>(namespaceDependencyRule, typeNameSet);
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
            _disallowRules.Add(namespaceDependencyRule);
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
            _visibleTypesByNamespace.AddOrUnion<Namespace, TypeNameSet, string>(ns, typeNameSet);
            return this;
        }

        private AnalyzerConfigBuilder AddVisibleTypesByNamespace(IEnumerable<KeyValuePair<Namespace, TypeNameSet>> visibleTypesByNamespace)
        {
            foreach (var keyValuePair in visibleTypesByNamespace)
                AddVisibleTypesByNamespace(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public AnalyzerConfigBuilder SetMaxIssueCount(int maxIssueCount)
        {
            _maxIssueCount = maxIssueCount;
            return this;
        }
    }
}