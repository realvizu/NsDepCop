using ICSharpCode.NRefactory.TypeSystem;

namespace Codartis.NsDepCop.Core.Analyzer.NRefactory
{
    // ReSharper disable once InconsistentNaming
    public static class ITypeExtensions
    {
        /// <summary>
        /// Returns the name of an NRefactory type in reflection style, eg. MyGeneric`1
        /// </summary>
        /// <param name="type">An NRefactory type object.</param>
        /// <returns>The name of the type in reflection style.</returns>
        public static string GetMetadataName(this IType type) =>
            type.TypeParameterCount == 0
            ? type.Name
            : $"{type.Name}`{type.TypeParameterCount}";
    }
}
