using System;
using System.Text;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a dependency rule between two domain specifications. Immutable.
    /// </summary>
    /// <remarks>
    /// The 'From' domain specification depends on the 'To' domain specification.
    /// A domain specification can represent more than just a single domain (eg. a subtree of namespaces).
    /// </remarks>
    [Serializable]
    public class DependencyRule
    {
        /// <summary>
        /// The dependency points from this domain to the other.
        /// </summary>
        public DomainSpecification From { get; }

        /// <summary>
        /// The dependency points into this domain.
        /// </summary>
        public DomainSpecification To { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="from">The source of the dependency.</param>
        /// <param name="to">The target of the dependency.</param>
        public DependencyRule(DomainSpecification from, DomainSpecification to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }

        /// <summary>
        /// Initializes a new instance by converting the string parameters to NamespaceSpecification objects.
        /// </summary>
        /// <param name="from">A namespace specification in string format. The source of the dependency.</param>
        /// <param name="to">A namespace specification in string format. The target of the dependency.</param>
        public DependencyRule(string from, string to)
            : this(DomainSpecificationParser.Parse(from), DomainSpecificationParser.Parse(to))
        { }

        /// <summary>
        /// Returns the string representation of a namespace dependency.
        /// </summary>
        /// <returns>The string representation of a namespace dependency.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(From);
            builder.Append("->");
            builder.Append(To);
            return builder.ToString();
        }

        public bool Equals(DependencyRule other)
        {
            return Equals(From, other.From) && Equals(To, other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DependencyRule)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((From != null ? From.GetHashCode() : 0) * 397) ^ (To != null ? To.GetHashCode() : 0);
            }
        }
    }
}
