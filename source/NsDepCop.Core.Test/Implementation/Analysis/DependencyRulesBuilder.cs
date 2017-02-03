using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
{
    /// <summary>
    /// Helper class for unit testing. Enables rule config building.
    /// </summary>
    internal class DependencyRulesBuilder : IDependencyRules
    {
        private bool _childCanDependOnParentImplicitly;
        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowedDependencies;
        private readonly HashSet<NamespaceDependencyRule> _disallowedDependencies;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesByTargetNamespace;

        public DependencyRulesBuilder()
        {
            _childCanDependOnParentImplicitly = ConfigDefaults.ChildCanDependOnParentImplicitly;
            _allowedDependencies = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowedDependencies = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByTargetNamespace = new Dictionary<Namespace, TypeNameSet>();
        }

        public bool ChildCanDependOnParentImplicitly => _childCanDependOnParentImplicitly;
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules => _allowedDependencies.ToImmutableDictionary();
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules => _disallowedDependencies.ToImmutableHashSet();
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace => _visibleTypesByTargetNamespace.ToImmutableDictionary();

        public DependencyRulesBuilder SetChildCanDependOnParentImplicitly(bool value)
        {
            _childCanDependOnParentImplicitly = value;
            return this;
        }

        public DependencyRulesBuilder AddAllowed(string from, string to, params string[] typeNames)
        {
            _allowedDependencies.Add(new NamespaceDependencyRule(from, to), new TypeNameSet(typeNames));
            return this;
        }

        public DependencyRulesBuilder AddDisallowed(string from, string to)
        {
            _disallowedDependencies.Add(new NamespaceDependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddVisibleMembers(string targetNamespace, params string[] typeNames)
        {
            _visibleTypesByTargetNamespace.Add(new Namespace(targetNamespace), new TypeNameSet(typeNames));
            return this;
        }
    }
}
