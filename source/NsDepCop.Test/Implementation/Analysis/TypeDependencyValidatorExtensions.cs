using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Implementation;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    internal static class TypeDependencyValidatorExtensions
    {
        private static readonly SourceSegment DummySourceSegment = new SourceSegment(0, 0, 0, 0, null, null);

        public static bool IsAllowedDependency(this TypeDependencyValidator typeDependencyValidator, 
            string fromNamespace, string fromType, string toNamespace, string toType)
        {
            var typeDependency = new TypeDependency(fromNamespace, fromType, toNamespace, toType, DummySourceSegment);
            return typeDependencyValidator.IsAllowedDependency(typeDependency).IsAllowed;
        }
    }
}
