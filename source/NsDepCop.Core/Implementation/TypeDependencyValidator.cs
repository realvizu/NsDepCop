using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Determines whether a given type-to-type dependency is allowed.
    /// </summary>
    public class TypeDependencyValidator : ITypeDependencyValidator
    {
        private readonly CachingNamespaceDependencyValidator _namespaceDependencyValidator;
        private readonly CachingTypeVisibilityValidator _typeVisibilityValidator;

        public TypeDependencyValidator(
            ImmutableHashSet<Dependency> allowedDependencies,
            ImmutableHashSet<Dependency> disallowedDependencies,
            bool childCanDependOnParentImplicitly,
            ImmutableDictionary<string, ImmutableHashSet<string>> visibleTypesPerNamespaces)
        {
            _namespaceDependencyValidator = new CachingNamespaceDependencyValidator(
                allowedDependencies, disallowedDependencies, childCanDependOnParentImplicitly);

            _typeVisibilityValidator = new CachingTypeVisibilityValidator(visibleTypesPerNamespaces);
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="sourceType">The name of the source type of the dependency.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <param name="targeType">The name of the type that the dependency points to.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public bool IsAllowedDependency(string fromNamespace, string sourceType, string toNamespace, string targeType)
        {
            // Inside a namespace dependencies are not restricted.
            if (fromNamespace == toNamespace)
                return true;

            return _namespaceDependencyValidator.IsAllowedDependency(fromNamespace, toNamespace)
                && _typeVisibilityValidator.IsTypeVisible(toNamespace, targeType);
        }

        public IEnumerable<CacheStatistics> GetCacheStatistics()
        {
            yield return new CacheStatistics(_namespaceDependencyValidator);
            yield return new CacheStatistics(_typeVisibilityValidator);
        }

        
    }
}
