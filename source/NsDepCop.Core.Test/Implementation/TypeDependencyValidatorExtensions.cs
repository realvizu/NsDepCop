using Codartis.NsDepCop.Core.Implementation;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Test.Implementation
{
    internal static class TypeDependencyValidatorExtensions
    {
        public static bool IsAllowedDependency(this TypeDependencyValidator typeDependencyValidator, 
            string fromNamespace, string fromType, string toNamespace, string toType)
        {
            var typeDependency = new TypeDependency(fromNamespace, fromType, toNamespace, toType);
            return typeDependencyValidator.IsAllowedDependency(typeDependency);
        }
    }
}
