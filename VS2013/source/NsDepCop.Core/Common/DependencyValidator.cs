using System.Collections.Immutable;

namespace Codartis.NsDepCop.Core.Common
{
    public class DependencyValidator
    {
        private readonly ImmutableHashSet<Dependency> _allowedDependencies;
        private readonly ImmutableHashSet<Dependency> _disallowedDependencies;

        public DependencyValidator(ImmutableHashSet<Dependency> allowedDependencies, ImmutableHashSet<Dependency> disallowedDependencies)
        {
            _allowedDependencies = allowedDependencies;
            _disallowedDependencies = disallowedDependencies;
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public bool IsAllowedDependency(string fromNamespace, string toNamespace)
        {
            return IsMathingDependencyFound(_allowedDependencies, fromNamespace, toNamespace) &&
                   !IsMathingDependencyFound(_disallowedDependencies, fromNamespace, toNamespace);

            // TODO: add caching
        }

        private static bool IsMathingDependencyFound(IImmutableSet<Dependency> dependencies, string fromNamespace, string toNamespace)
        {
            foreach (var fromCandidate in NamespaceSpecification.GetContainingNamespaceSpecifications(fromNamespace))
            {
                foreach (var toCandidate in NamespaceSpecification.GetContainingNamespaceSpecifications(toNamespace))
                {
                    Dependency foundDependency;
                    if (dependencies.TryGetValue(new Dependency(fromCandidate, toCandidate), out foundDependency))
                    {
                        //Debug.WriteLine(string.Format("Dependency from '{0}' to '{1}' is allowed by rule '{2}'",
                        //    fromNamespace, toNamespace, foundDependency), Constants.TOOL_NAME);

                        // A matching rule was found.
                        return true;
                    }
                }
            }

            // No matching rule was found.
            return false;
        }
    }
}
