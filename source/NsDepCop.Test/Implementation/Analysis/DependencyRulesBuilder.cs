using System.Collections.Generic;
using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    /// <summary>
    /// Helper class for unit testing. Enables rule config building.
    /// </summary>
    internal class DependencyRulesBuilder : IDependencyRules
    {
        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowedDependencies;
        private readonly HashSet<NamespaceDependencyRule> _disallowedDependencies;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesByTargetNamespace;
        private readonly HashSet<NamespaceDependencyRule> _allowedAssemblyDependencies;
        private readonly HashSet<NamespaceDependencyRule> _disallowedAssemblyDependencies;

        public DependencyRulesBuilder()
        {
            ChildCanDependOnParentImplicitly = ConfigDefaults.ChildCanDependOnParentImplicitly;
            ParentCanDependOnChildImplicitly = ConfigDefaults.ParentCanDependOnChildImplicitly;
            _allowedDependencies = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowedDependencies = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByTargetNamespace = new Dictionary<Namespace, TypeNameSet>();
            _allowedAssemblyDependencies = new HashSet<NamespaceDependencyRule>();
            _disallowedAssemblyDependencies = new HashSet<NamespaceDependencyRule>();
        }

        public bool ChildCanDependOnParentImplicitly { get; private set; }

        public bool ParentCanDependOnChildImplicitly { get; }

        public Dictionary<NamespaceDependencyRule, TypeNameSet> AllowRules => _allowedDependencies;
        public HashSet<NamespaceDependencyRule> DisallowRules => _disallowedDependencies;
        public Dictionary<Namespace, TypeNameSet> VisibleTypesByNamespace => _visibleTypesByTargetNamespace;
        public HashSet<NamespaceDependencyRule> AllowedAssemblyRules => _allowedAssemblyDependencies;
        public HashSet<NamespaceDependencyRule> DisallowedAssemblyRules => _disallowedAssemblyDependencies;

        public DependencyRulesBuilder SetChildCanDependOnParentImplicitly(bool value)
        {
            ChildCanDependOnParentImplicitly = value;
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

        public DependencyRulesBuilder AddAllowedAssemblyDependency(string from, string to)
        {
            _allowedAssemblyDependencies.Add(new NamespaceDependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddDisallowedAssemblyDependency(string from, string to)
        {
            _disallowedAssemblyDependencies.Add(new NamespaceDependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddVisibleMembers(string targetNamespace, params string[] typeNames)
        {
            _visibleTypesByTargetNamespace.Add(new Namespace(targetNamespace), new TypeNameSet(typeNames));
            return this;
        }
    }
}
