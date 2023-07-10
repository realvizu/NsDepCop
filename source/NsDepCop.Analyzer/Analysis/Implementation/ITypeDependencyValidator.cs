namespace Codartis.NsDepCop.Analysis.Implementation
{
    /// <summary>
    /// Determines whether a type-to-type dependency is allowed or not.
    /// </summary>
    internal interface ITypeDependencyValidator
    {
        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="typeDependency">A dependency of two types.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        DependencyStatus IsAllowedDependency(TypeDependency typeDependency);
    }
}