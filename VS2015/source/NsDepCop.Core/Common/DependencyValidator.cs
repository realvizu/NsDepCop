using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Validates dependencies to a set of allowed/disallowed rules.
    /// </summary>
    public class DependencyValidator
    {
        private readonly ImmutableHashSet<Dependency> _allowedDependencies;
        private readonly ImmutableHashSet<Dependency> _disallowedDependencies;
        private readonly Dictionary<Dependency, bool> _alreadyAnalyzedDependencies;
        private readonly bool _childCanDependOnParentImplicitly;

        public int CacheHitCount { get; private set; }
        public int CacheMissCount { get; private set; }

        public DependencyValidator(
            ImmutableHashSet<Dependency> allowedDependencies, 
            ImmutableHashSet<Dependency> disallowedDependencies,
            bool childCanDependOnParentImplicitly = false)
        {
            _allowedDependencies = allowedDependencies;
            _disallowedDependencies = disallowedDependencies;
            _alreadyAnalyzedDependencies = new Dictionary<Dependency, bool>();
            _childCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            CacheHitCount = 0;
            CacheMissCount = 0;
        }

        public double CacheEfficiencyPercent
        {
            get
            {
                var totalCount = CacheHitCount + CacheMissCount;
                return totalCount == 0 ? 0 : (double) CacheHitCount/totalCount;
            }
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public bool IsAllowedDependency(string fromNamespace, string toNamespace)
        {
            if (fromNamespace == toNamespace)
                return true;

            var dependency = new Dependency(fromNamespace, toNamespace);

            bool isAllowed;
            if (_alreadyAnalyzedDependencies.TryGetValue(dependency, out isAllowed))
            {
                CacheHitCount++;

                Debug.WriteLine(
                    $"Cache hit: dependency {dependency} is {(isAllowed ? "allowed" : "disallowed")}.",
                    Constants.TOOL_NAME);
            }
            else
            {
                CacheMissCount++;

                isAllowed = (IsAllowedBecauseChildCanDependOnParent(dependency.From, dependency.To) 
                    || IsMathingDependencyFound(_allowedDependencies, dependency.From, dependency.To, "allowed"))
                    && !IsMathingDependencyFound(_disallowedDependencies, dependency.From, dependency.To, "disallowed");

                _alreadyAnalyzedDependencies.Add(dependency, isAllowed);

                Debug.WriteLine($"Dependency {dependency} added to cache as {(isAllowed ? "allowed" : "disallowed")}.", 
                    Constants.TOOL_NAME);
            }

            return isAllowed;
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
    }
}
