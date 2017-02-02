using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Builds project config objects.
    /// </summary>
    internal class ProjectConfigBuilder
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

        public ProjectConfigBuilder(Parsers? overridingParser = null)
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

        public ProjectConfigBuilder(IProjectConfig projectConfig)
        {
            _isEnabled = projectConfig.IsEnabled;
            _issueKind = projectConfig.IssueKind;
            _infoImportance = projectConfig.InfoImportance;
            _parser = projectConfig.Parser;

            _childCanDependOnParentImplicitly = projectConfig.ChildCanDependOnParentImplicitly;
            _allowRules = projectConfig.AllowRules.ToDictionary(i => i.Key, i => i.Value);
            _disallowRules = projectConfig.DisallowRules.ToHashSet();
            _visibleTypesByNamespace = projectConfig.VisibleTypesByNamespace.ToDictionary(i => i.Key, i => i.Value);
            _maxIssueCount = projectConfig.MaxIssueCount;
        }

        public IProjectConfig ToProjectConfig()
        {
            return new ProjectConfig(
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

        public ProjectConfigBuilder Combine(IProjectConfig projectConfig)
        {
            SetIsEnabled(projectConfig.IsEnabled);
            SetIssueKind(projectConfig.IssueKind);
            SetInfoImportance(projectConfig.InfoImportance);
            SetParser(projectConfig.Parser);

            SetChildCanDependOnParentImplicitly(projectConfig.ChildCanDependOnParentImplicitly);
            AddAllowRules(projectConfig.AllowRules);
            AddDisallowRules(projectConfig.DisallowRules);
            AddVisibleTypesByNamespace(projectConfig.VisibleTypesByNamespace);
            SetMaxIssueCount(projectConfig.MaxIssueCount);

            return this;
        }

        public ProjectConfigBuilder SetIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
            return this;
        }

        public ProjectConfigBuilder SetIssueKind(IssueKind issueKind)
        {
            _issueKind = issueKind;
            return this;
        }

        public ProjectConfigBuilder SetInfoImportance(Importance infoImportance)
        {
            _infoImportance = infoImportance;
            return this;
        }

        public ProjectConfigBuilder SetParser(Parsers parser)
        {
            if (!_isParserOverridden)
                _parser = parser;

            return this;
        }

        public ProjectConfigBuilder SetChildCanDependOnParentImplicitly(bool childCanDependOnParentImplicitly)
        {
            _childCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            return this;
        }

        public ProjectConfigBuilder AddAllowRule(NamespaceDependencyRule namespaceDependencyRule, TypeNameSet typeNameSet = null)
        {
            _allowRules.AddOrUnion<NamespaceDependencyRule, TypeNameSet, string>(namespaceDependencyRule, typeNameSet);
            return this;
        }

        private ProjectConfigBuilder AddAllowRules(IEnumerable<KeyValuePair<NamespaceDependencyRule, TypeNameSet>> allowRules)
        {
            foreach (var keyValuePair in allowRules)
                AddAllowRule(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public ProjectConfigBuilder AddDisallowRule(NamespaceDependencyRule namespaceDependencyRule)
        {
            _disallowRules.Add(namespaceDependencyRule);
            return this;
        }

        private ProjectConfigBuilder AddDisallowRules(IEnumerable<NamespaceDependencyRule> disallowRules)
        {
            foreach (var namespaceDependencyRule in disallowRules)
                AddDisallowRule(namespaceDependencyRule);
            return this;
        }

        public ProjectConfigBuilder AddVisibleTypesByNamespace(Namespace ns, TypeNameSet typeNameSet)
        {
            _visibleTypesByNamespace.AddOrUnion<Namespace, TypeNameSet, string>(ns, typeNameSet);
            return this;
        }

        private ProjectConfigBuilder AddVisibleTypesByNamespace(IEnumerable<KeyValuePair<Namespace, TypeNameSet>> visibleTypesByNamespace)
        {
            foreach (var keyValuePair in visibleTypesByNamespace)
                AddVisibleTypesByNamespace(keyValuePair.Key, keyValuePair.Value);
            return this;
        }

        public ProjectConfigBuilder SetMaxIssueCount(int maxIssueCount)
        {
            _maxIssueCount = maxIssueCount;
            return this;
        }
    }
}