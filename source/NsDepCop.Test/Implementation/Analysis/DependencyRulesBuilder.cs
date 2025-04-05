using System.Collections.Generic;
using Codartis.NsDepCop.Config;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    /// <summary>
    /// Helper class for unit testing. Enables rule config building.
    /// </summary>
    public class DependencyRulesBuilder : IDependencyRules
    {
        private readonly Dictionary<DependencyRule, TypeNameSet> _allowedDependencies;
        private readonly HashSet<DependencyRule> _disallowedDependencies;
        private readonly Dictionary<Domain, TypeNameSet> _visibleTypesByTargetNamespace;
        private readonly HashSet<DependencyRule> _allowedAssemblyDependencies;
        private readonly HashSet<DependencyRule> _disallowedAssemblyDependencies;

        public DependencyRulesBuilder()
        {
            ChildCanDependOnParentImplicitly = ConfigDefaults.ChildCanDependOnParentImplicitly;
            ParentCanDependOnChildImplicitly = ConfigDefaults.ParentCanDependOnChildImplicitly;
            _allowedDependencies = new Dictionary<DependencyRule, TypeNameSet>();
            _disallowedDependencies = new HashSet<DependencyRule>();
            _visibleTypesByTargetNamespace = new Dictionary<Domain, TypeNameSet>();
            _allowedAssemblyDependencies = new HashSet<DependencyRule>();
            _disallowedAssemblyDependencies = new HashSet<DependencyRule>();
        }

        public bool ChildCanDependOnParentImplicitly { get; private set; }

        public bool ParentCanDependOnChildImplicitly { get; }

        public Dictionary<DependencyRule, TypeNameSet> AllowRules => _allowedDependencies;
        public HashSet<DependencyRule> DisallowRules => _disallowedDependencies;
        public Dictionary<Domain, TypeNameSet> VisibleTypesByNamespace => _visibleTypesByTargetNamespace;
        public HashSet<DependencyRule> AllowedAssemblyRules => _allowedAssemblyDependencies;
        public HashSet<DependencyRule> DisallowedAssemblyRules => _disallowedAssemblyDependencies;

        public DependencyRulesBuilder SetChildCanDependOnParentImplicitly(bool value)
        {
            ChildCanDependOnParentImplicitly = value;
            return this;
        }

        public DependencyRulesBuilder AddAllowed(DomainSpecification from, DomainSpecification to, params string[] typeNames)
        {
            _allowedDependencies.Add(new DependencyRule(from, to), new TypeNameSet(typeNames));
            return this;
        }

        public DependencyRulesBuilder AddAllowed(string from, string to, params string[] typeNames)
        {
            _allowedDependencies.Add(new DependencyRule(from, to), new TypeNameSet(typeNames));
            return this;
        }

        public DependencyRulesBuilder AddDisallowed(string from, string to)
        {
            _disallowedDependencies.Add(new DependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddAllowedAssemblyDependency(string from, string to)
        {
            _allowedAssemblyDependencies.Add(new DependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddDisallowedAssemblyDependency(string from, string to)
        {
            _disallowedAssemblyDependencies.Add(new DependencyRule(from, to));
            return this;
        }

        public DependencyRulesBuilder AddVisibleMembers(string targetNamespace, params string[] typeNames)
        {
            _visibleTypesByTargetNamespace.Add(new Domain(targetNamespace), new TypeNameSet(typeNames));
            return this;
        }
    }
}
