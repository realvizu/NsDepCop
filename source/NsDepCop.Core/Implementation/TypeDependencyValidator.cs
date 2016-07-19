using System.Collections.Immutable;
using System.Linq;
using Codartis.NsDepCop.Core.Interface;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Determines whether a given type-to-type dependency is allowed.
    /// </summary>
    public class TypeDependencyValidator : ITypeDependencyValidator
    {
        private readonly ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> _allowRules;
        private readonly ImmutableHashSet<NamespaceDependencyRule> _disallowRules;
        private readonly ImmutableDictionary<Namespace, TypeNameSet> _visibleTypesPerNamespaces;
        private readonly bool _childCanDependOnParentImplicitly;

        public TypeDependencyValidator(IRuleConfig ruleConfig)
        {
            _allowRules = ruleConfig.AllowRules;
            _disallowRules = ruleConfig.DisallowRules;
            _visibleTypesPerNamespaces = ruleConfig.VisibleTypesByNamespace;
            _childCanDependOnParentImplicitly = ruleConfig.ChildCanDependOnParentImplicitly;
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="typeDependency">A dependency of two types.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public virtual bool IsAllowedDependency(TypeDependency typeDependency)
        {
            // Inside a namespace all dependencies are allowed.
            if (typeDependency.FromNamespaceName == typeDependency.ToNamespaceName)
                return true;

            var fromNamespace = new Namespace(typeDependency.FromNamespaceName);
            var toNamespace = new Namespace(typeDependency.ToNamespaceName);

            var disallowRule = GetDisallowRule(fromNamespace, toNamespace);
            if (disallowRule != null)
                return false;

            if (IsAllowedBecauseChildCanDependOnParent(fromNamespace, toNamespace))
                return true;

            var allowRule = GetMostSpecificAllowRule(fromNamespace, toNamespace);
            if (allowRule == null)
                return false;

            var visibleMembers = GetVisibleMembers(allowRule, toNamespace);
            if (!visibleMembers.EmptyIfNull().Any())
                return true;

            return visibleMembers.Contains(typeDependency.ToTypeName);
        }

        public bool IsAllowedDependency(string fromNamespace, string fromType, string toNamespace, string toType)
            => IsAllowedDependency(new TypeDependency(fromNamespace, fromType, toNamespace, toType));

        private bool IsAllowedBecauseChildCanDependOnParent(Namespace fromNamespace, Namespace toNamespace)
        {
            return _childCanDependOnParentImplicitly && fromNamespace.IsSubnamespaceOf(toNamespace);
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
                allowedTypeNameSet.EmptyIfNull().Any())
                return allowedTypeNameSet;

            if (_visibleTypesPerNamespaces.TryGetValue(targetNamespace, out allowedTypeNameSet))
                return allowedTypeNameSet;

            return null;
        }
    }
}
