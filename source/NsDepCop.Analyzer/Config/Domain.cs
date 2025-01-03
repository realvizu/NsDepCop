using System;
using System.Linq;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a domain, eg. 'A.B'. Immutable.
    /// </summary>
    /// <remarks>
    /// The global domain is also a domain and it's represented by '.' (a dot)
    /// </remarks>
    [Serializable]
    public sealed class Domain : DomainSpecification
    {
        public const string RootDomainMarker = ".";

        /// <summary>
        /// Represents the global domain.
        /// </summary>
        public static readonly Domain GlobalDomain = new Domain(RootDomainMarker);

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="domainAsString">The string representation of a domain.</param>
        /// <param name="validate">True means validate the input string.</param>
        public Domain(string domainAsString, bool validate = true)
            : base(Normalize(domainAsString), validate, IsValid)
        {
        }

        public override int GetMatchRelevance(Domain domain)
        {
            return this == domain
                ? int.MaxValue
                : 0;
        }

        /// <summary>
        /// Determines whether this domain is a sub-domain of the given other one.
        /// </summary>
        /// <param name="parentCandidate">The domain to test whether it's a parent of the current domain.</param>
        /// <returns>True if this domain is a sub-domain of the given one, false otherwise.</returns>
        public bool IsSubDomain(Domain parentCandidate)
        {
            if (this == GlobalDomain)
                return false;

            if (parentCandidate == GlobalDomain)
                return true;

            var parentPrefix = parentCandidate.Value + DomainPartSeparator;
            return Value.StartsWith(parentPrefix);
        }

        /// <summary>
        /// Validates that the given string represents a valid domain specification.
        /// </summary>
        /// <param name="domainAsString">A domain specification in string format.</param>
        /// <returns>True if the given string represents a valid domain specification.</returns>
        public static bool IsValid(string domainAsString)
        {
            if (domainAsString == RootDomainMarker)
                return true;

            if (domainAsString.Any(c => c == WildcardDomain.AnyDomainMarker[0] || c == WildcardDomain.SingleDomainMarker[0]))
                return false;

            var pieces = domainAsString.Split(new[] { DomainPartSeparator }, StringSplitOptions.None);

            return pieces.All(i => !string.IsNullOrWhiteSpace(i));
        }

        /// <summary>
        /// Converts the input string into a standard representation (removes ambiguities).
        /// </summary>
        /// <param name="domainAsString">A domain specification in string format.</param>
        /// <returns>The standard representation of the given domain specification string.</returns>
        private static string Normalize(string domainAsString)
        {
            // Global domain representations:
            //   Roslyn: "<global namespace>"
            //   NRefactory: "" (empty string)
            //   NsDepCop: "." (dot)
            if (domainAsString == "" ||
                domainAsString == "<global namespace>")
                domainAsString = RootDomainMarker;

            return domainAsString;
        }
    }
}
