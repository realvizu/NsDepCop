using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    /// <summary>
    /// Determines whether a given type-to-type dependency is allowed.
    /// </summary>
    public class TypeDependencyValidator : ITypeDependencyValidator
    {
        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowRules;
        private readonly HashSet<NamespaceDependencyRule> _disallowRules;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesPerNamespaces;
        private readonly bool _childCanDependOnParentImplicitly;
        private readonly bool _parentCanDependOnChildImplicitly;

        public TypeDependencyValidator(IDependencyRules dependencyRules)
        {
            _allowRules = dependencyRules.AllowRules;
            _disallowRules = dependencyRules.DisallowRules;
            _visibleTypesPerNamespaces = dependencyRules.VisibleTypesByNamespace;
            _childCanDependOnParentImplicitly = dependencyRules.ChildCanDependOnParentImplicitly;
            _parentCanDependOnChildImplicitly = dependencyRules.ParentCanDependOnChildImplicitly;
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="typeDependency">A dependency of two types.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public virtual DependencyStatus IsAllowedDependency(TypeDependency typeDependency)
        {
            // Inside a namespace all dependencies are allowed.
            if (typeDependency.FromNamespaceName == typeDependency.ToNamespaceName)
                return DependencyStatus.Allowed;

            // These namespace names are coming from a compiler so we don't have to validate them.
            var fromNamespace = new Namespace(typeDependency.FromNamespaceName, validate: false);
            var toNamespace = new Namespace(typeDependency.ToNamespaceName, validate: false);

            var disallowRule = GetDisallowRule(fromNamespace, toNamespace);
            if (disallowRule != null)
                return DependencyStatus.Disallowed;

            if (IsAllowedBecauseChildCanDependOnParent(fromNamespace, toNamespace))
                return DependencyStatus.Allowed;
            
            if (IsAllowedBecauseParentCanDependOnChild(fromNamespace, toNamespace))
                return DependencyStatus.Allowed;

            var allowRule = GetMostSpecificAllowRule(fromNamespace, toNamespace);
            if (allowRule == null)
                return DependencyStatus.Disallowed;

            TypeNameSet visibleMembers = GetVisibleMembers(allowRule, toNamespace);
            if (visibleMembers == null || visibleMembers.Count == 0)
                return DependencyStatus.Allowed;

            bool isUsingVisibleMember = visibleMembers.Contains(typeDependency.ToTypeName);
            
            return isUsingVisibleMember
                ? DependencyStatus.Allowed
                : DependencyStatus.DisallowedUseOfMember(visibleMembers.ToArray());
        }

        private bool IsAllowedBecauseChildCanDependOnParent(Namespace fromNamespace, Namespace toNamespace)
        {
            return _childCanDependOnParentImplicitly && fromNamespace.IsSubnamespaceOf(toNamespace);
        }
        
        private bool IsAllowedBecauseParentCanDependOnChild(Namespace fromNamespace, Namespace toNamespace)
        {
            return _parentCanDependOnChildImplicitly && toNamespace.IsSubnamespaceOf(fromNamespace);
        }

        private NamespaceDependencyRule GetMostSpecificAllowRule(Namespace from, Namespace to)
        {
            return _allowRules.Keys
                .Where(i => i.From.Matches(from) && i.To.Matches(to))
                .MaxByOrDefault(i => i.From.GetMatchRelevance(from));
        }

        private NamespaceDependencyRule GetDisallowRule(Namespace from, Namespace to)
        {
            return _disallowRules
                .FirstOrDefault(i => i.From.Matches(from) && i.To.Matches(to));
        }

        private TypeNameSet GetVisibleMembers(NamespaceDependencyRule allowRule, Namespace targetNamespace)
        {
            TypeNameSet allowedTypeNameSet;

            if (_allowRules.TryGetValue(allowRule, out allowedTypeNameSet) &&
                allowedTypeNameSet != null &&
                allowedTypeNameSet.Any())
                return allowedTypeNameSet;

            if (_visibleTypesPerNamespaces.TryGetValue(targetNamespace, out allowedTypeNameSet))
                return allowedTypeNameSet;

            return null;
        }
    }
}
