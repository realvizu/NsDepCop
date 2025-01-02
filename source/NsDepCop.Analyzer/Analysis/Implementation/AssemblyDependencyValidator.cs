using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    public sealed class AssemblyDependencyValidator : IAssemblyDependencyValidator
    {
        private readonly HashSet<DependencyRule> _allowRules;
        private readonly HashSet<DependencyRule> _disallowRules;

        public AssemblyDependencyValidator(IDependencyRules dependencyRules)
        {
            if (dependencyRules is null) throw new ArgumentNullException(nameof(dependencyRules));

            _allowRules = dependencyRules.AllowedAssemblyRules;
            _disallowRules = dependencyRules.DisallowedAssemblyRules;
        }

        public DependencyStatus IsDependencyAllowed(AssemblyDependency assemblyDependency)
        {
            if (assemblyDependency == AssemblyDependency.Empty)
            {
                throw new ArgumentException("The parameter is empty.", nameof(assemblyDependency));
            }

            // These assembly names are coming from a compiler so we don't have to validate them.
            var fromAssembly = new Domain(assemblyDependency.FromAssembly.Name, validate: false);
            var toAssembly = new Domain(assemblyDependency.ToAssembly.Name, validate: false);

            var disallowRule = GetDisallowRule(fromAssembly, toAssembly);
            if (disallowRule is not null)
                return DependencyStatus.Disallowed;

            var allowRule = GetMostSpecificAllowRule(fromAssembly, toAssembly);
            if (allowRule is null)
                return DependencyStatus.Disallowed;

            return DependencyStatus.Allowed;
        }

        private DependencyRule GetMostSpecificAllowRule(Domain from, Domain to)
        {
            return _allowRules
                .Where(element => element.From.Matches(from) && element.To.Matches(to))
                .MaxByOrDefault(element => element.From.GetMatchRelevance(from));
        }

        private DependencyRule GetDisallowRule(Domain from, Domain to)
        {
            return _disallowRules.FirstOrDefault(element => element.From.Matches(from) && element.To.Matches(to));
        }
    }
}
