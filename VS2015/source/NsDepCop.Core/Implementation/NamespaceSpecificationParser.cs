namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Converts a string to a namespace specification.
    /// </summary>
    public static class NamespaceSpecificationParser
    {
        /// <summary>
        /// Creates a namespace specification from a string representation.
        /// Throws an exception if the string cannot be parsed.
        /// </summary>
        /// <param name="namespaceSpecificationAsString">A namespace specification in string format.</param>
        /// <returns>The namespace specification created from the given string.</returns>
        public static NamespaceSpecification Parse(string namespaceSpecificationAsString)
        {
            return namespaceSpecificationAsString.EndsWith(NamespaceTree.ANY_NAMESPACE_MARKER) 
                ? (NamespaceSpecification) new NamespaceTree(namespaceSpecificationAsString) 
                : new Namespace(namespaceSpecificationAsString);
        }
    }
}