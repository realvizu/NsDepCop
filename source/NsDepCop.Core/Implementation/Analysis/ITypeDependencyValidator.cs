using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Determines whether a type-to-type dependency is allowed or not.
    /// </summary>
    public interface ITypeDependencyValidator
    {
        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="typeDependency">A dependency of two types.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        bool IsAllowedDependency(TypeDependency typeDependency);
    }
}