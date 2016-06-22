using System.Collections.Immutable;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Validates dependencies to a set of allowed/disallowed rules.
    /// </summary>
    public class NamespaceDependencyValidator
    {
        private readonly ImmutableHashSet<Dependency> _allowedDependencies;
        private readonly ImmutableHashSet<Dependency> _disallowedDependencies;
        private readonly bool _childCanDependOnParentImplicitly;

        public NamespaceDependencyValidator(
            ImmutableHashSet<Dependency> allowedDependencies, 
            ImmutableHashSet<Dependency> disallowedDependencies,
            bool childCanDependOnParentImplicitly = false)
        {
            _allowedDependencies = allowedDependencies;
            _disallowedDependencies = disallowedDependencies;
            _childCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public virtual bool IsAllowedDependency(string fromNamespace, string toNamespace)
        {
            if (fromNamespace == toNamespace)
                return true;

            var dependency = new Dependency(fromNamespace, toNamespace);

            var isNamespaceDependencyAllowed = (IsAllowedBecauseChildCanDependOnParent(dependency.From, dependency.To) 
                    || IsMathingDependencyFound(_allowedDependencies, dependency.From, dependency.To, IsAllowedToString(true)))
                    && !IsMathingDependencyFound(_disallowedDependencies, dependency.From, dependency.To, IsAllowedToString(false));

            return isNamespaceDependencyAllowed;
        }

        private bool IsAllowedBecauseChildCanDependOnParent(NamespaceSpecification fromNamespace, NamespaceSpecification toNamespace)
        {
            return _childCanDependOnParentImplicitly && fromNamespace.IsSubnamespaceOf(toNamespace);
        }

        private static bool IsMathingDependencyFound(IImmutableSet<Dependency> dependencies, 
            NamespaceSpecification fromNamespace, NamespaceSpecification toNamespace, string debugString)
        {
            foreach (var fromCandidate in fromNamespace.GetContainingNamespaceSpecifications())
            {
                foreach (var toCandidate in toNamespace.GetContainingNamespaceSpecifications())
                {
                    Dependency foundDependency;
                    if (dependencies.TryGetValue(new Dependency(fromCandidate, toCandidate), out foundDependency))
                    {
                        Debug.WriteLine(string.Format("Dependency from '{0}' to '{1}' is {3} by rule '{2}'.",
                            fromNamespace, toNamespace, foundDependency, debugString), Constants.TOOL_NAME);

                        // A matching rule was found.
                        return true;
                    }
                }
            }

            // No matching rule was found.
            return false;
        }

        protected static string IsAllowedToString(bool isAllowed)
        {
            return isAllowed ? "allowed" : "disallowed";
        }
    }
}
