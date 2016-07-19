using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Implementation;

namespace Codartis.NsDepCop.Core.Test.Implementation
{
    /// <summary>
    /// Helper class for unit testing. Enables rule config building.
    /// </summary>
    internal class RuleConfigBuilder : IRuleConfig
    {
        private bool _childCanDependOnParentImplicitly;
        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowedDependencies;
        private readonly HashSet<NamespaceDependencyRule> _disallowedDependencies;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesByTargetNamespace;

        public RuleConfigBuilder()
        {
            _childCanDependOnParentImplicitly = true;
            _allowedDependencies = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowedDependencies = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByTargetNamespace = new Dictionary<Namespace, TypeNameSet>();
        }

        public bool ChildCanDependOnParentImplicitly => _childCanDependOnParentImplicitly;
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules => _allowedDependencies.ToImmutableDictionary();
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules => _disallowedDependencies.ToImmutableHashSet();
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace => _visibleTypesByTargetNamespace.ToImmutableDictionary();

        public RuleConfigBuilder SetChildCanDependOnParentImplicitly(bool value)
        {
            _childCanDependOnParentImplicitly = value;
            return this;
        }

        public RuleConfigBuilder AddAllowed(string from, string to, params string[] typeNames)
        {
            _allowedDependencies.Add(new NamespaceDependencyRule(from, to), new TypeNameSet(typeNames));
            return this;
        }

        public RuleConfigBuilder AddDisallowed(string from, string to)
        {
            _disallowedDependencies.Add(new NamespaceDependencyRule(from, to));
            return this;
        }

        public RuleConfigBuilder AddVisibleMembers(string targetNamespace, params string[] typeNames)
        {
            _visibleTypesByTargetNamespace.Add(new Namespace(targetNamespace), new TypeNameSet(typeNames));
            return this;
        }
    }
}
