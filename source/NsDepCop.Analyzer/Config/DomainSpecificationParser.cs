namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Converts a string to a domain specification.
    /// </summary>
    public static class DomainSpecificationParser
    {
        /// <summary>
        /// Creates a domain specification from a string representation.
        /// </summary>
        /// <param name="domainSpecificationAsString">A domain specification in string format.</param>
        /// <returns>The domain specification created from the given string.</returns>
        /// <remarks>
        /// Throws an exception if the string cannot be parsed.
        /// </remarks>
        public static DomainSpecification Parse(string domainSpecificationAsString)
        {
            if (domainSpecificationAsString.StartsWith(RegexDomain.Delimiter) && domainSpecificationAsString.EndsWith(RegexDomain.Delimiter))
                return new RegexDomain(domainSpecificationAsString);
            if (domainSpecificationAsString.Contains(WildcardDomain.SingleDomainMarker) || domainSpecificationAsString.Contains(WildcardDomain.AnyDomainMarker))
                return new WildcardDomain(domainSpecificationAsString);
            return new Domain(domainSpecificationAsString);
        }
    }
}