using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    public sealed class AssemblyDependencyValidator : IAssemblyDependencyValidator
    {
        private readonly HashSet<NamespaceDependencyRule> _allowRules;
        private readonly HashSet<NamespaceDependencyRule> _disallowRules;

        public AssemblyDependencyValidator(IDependencyRules dependencyRules)
        {
            if (dependencyRules is null) throw new ArgumentNullException(nameof(dependencyRules));

            _allowRules = dependencyRules.AllowAssemblyRules;
            _disallowRules = dependencyRules.DisallowAssemblyRules;
        }

        public DependencyStatus IsDependencyAllowed(AssemblyDependency assemblyDependency)
        {
            if (assemblyDependency == AssemblyDependency.Empty)
            {
                throw new ArgumentException("The parameter is empty.", nameof(assemblyDependency));
            }

            // Inside a assembly all dependencies are allowed.
            if (assemblyDependency.FromAssembly == assemblyDependency.ToAssembly)
                return DependencyStatus.Allowed;

            // These assembly names are coming from a compiler so we don't have to validate them.
            var fromAssembly = new Namespace(assemblyDependency.FromAssembly.Name, validate: false);
            var toAssembly = new Namespace(assemblyDependency.ToAssembly.Name, validate: false);

            var disallowRule = GetDisallowRule(fromAssembly, toAssembly);
            if (disallowRule is not null)
                return DependencyStatus.Disallowed;

            var allowRule = GetMostSpecificAllowRule(fromAssembly, toAssembly);
            if (allowRule is null)
                return DependencyStatus.Disallowed;

            return DependencyStatus.Allowed;
        }

        private NamespaceDependencyRule GetMostSpecificAllowRule(Namespace from, Namespace to)
        {
            return _allowRules
                .Where(element => element.From.Matches(from) && element.To.Matches(to))
                .MaxByOrDefault(element => element.From.GetMatchRelevance(from));
        }

        private NamespaceDependencyRule GetDisallowRule(Namespace from, Namespace to)
        {
            return _disallowRules.FirstOrDefault(element => element.From.Matches(from) && element.To.Matches(to));
        }
    }
}
