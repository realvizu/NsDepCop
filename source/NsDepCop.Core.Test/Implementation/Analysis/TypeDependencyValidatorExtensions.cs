using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
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
