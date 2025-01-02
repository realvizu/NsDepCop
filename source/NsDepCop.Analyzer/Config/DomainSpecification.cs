using System;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a domain or a domain pattern. Immutable.
    /// </summary>
    [Serializable]
    public abstract class DomainSpecification
    {
        public const char DomainPartSeparator = '.';

        /// <summary>
        /// The domain specification stored as a string.
        /// </summary>
        protected readonly string DomainSpecificationAsString;

        /// <summary>
        /// Initializes a new instance. Also validates the input format if needed.
        /// </summary>
        /// <param name="domainSpecificationAsString">The string representation of the domain specification.</param>
        /// <param name="validate">True means validate the input string.</param>
        /// <param name="validator">A delegate that validates the input string.</param>
        protected DomainSpecification(string domainSpecificationAsString, bool validate, Func<string, bool> validator)
        {
            if (domainSpecificationAsString == null)
                throw new ArgumentNullException(nameof(domainSpecificationAsString));

            if (validate &&
                validator != null &&
                !validator.Invoke(domainSpecificationAsString))
                throw new FormatException($"'{domainSpecificationAsString}' is not a valid {GetType().Name}.");

            DomainSpecificationAsString = domainSpecificationAsString;
        }

        /// <summary>
        /// Returns a number indicating how well this domain specification matches a concrete domain.
        /// </summary>
        /// <param name="domain">A domain.</param>
        /// <returns>Zero means no match. Higher value means more relevant match.</returns>
        public abstract int GetMatchRelevance(Domain domain);

        /// <summary>
        /// Returns a value indicating whether this domain specification matches a given domain.
        /// </summary>
        /// <param name="domain">A domain.</param>
        /// <returns>True if this domain specification matches the given domain.</returns>
        public bool Matches(Domain domain) => GetMatchRelevance(domain) > 0;

        public override string ToString() => DomainSpecificationAsString;

        public bool Equals(DomainSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return String.Equals(DomainSpecificationAsString, other.DomainSpecificationAsString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DomainSpecification)obj);
        }

        public override int GetHashCode()
        {
            return (DomainSpecificationAsString != null ? DomainSpecificationAsString.GetHashCode() : 0);
        }

        public static bool operator ==(DomainSpecification left, DomainSpecification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DomainSpecification left, DomainSpecification right)
        {
            return !Equals(left, right);
        }
    }
}
