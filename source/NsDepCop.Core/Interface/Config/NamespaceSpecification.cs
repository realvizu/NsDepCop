using System;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Represents a namespace or a namespace tree.
    /// </summary>
    public abstract class NamespaceSpecification
    {
        public const char NamespacePartSeparator = '.';

        /// <summary>
        /// The namespace specification stored as a string.
        /// </summary>
        protected readonly string NamespaceSpecificationAsString;

        /// <summary>
        /// Initializes a new instance. Also validates the input format if needed.
        /// </summary>
        /// <param name="namespaceSpecificationAsString">The string representation of the namespace specification.</param>
        /// <param name="validate">True means validate the input string.</param>
        /// <param name="validator">A delegate that validates the input string.</param>
        protected NamespaceSpecification(string namespaceSpecificationAsString, bool validate, Func<string, bool> validator)
        {
            if (namespaceSpecificationAsString == null)
                throw new ArgumentNullException(nameof(namespaceSpecificationAsString));

            if (validate && 
                validator != null && 
                !validator.Invoke(namespaceSpecificationAsString))
                throw new FormatException($"'{namespaceSpecificationAsString}' is not a valid {GetType().Name}.");

            NamespaceSpecificationAsString = namespaceSpecificationAsString;
        }

        /// <summary>
        /// Returns a number indicating how well this namespaces specification matches a concrete namespace.
        /// </summary>
        /// <param name="ns">A namespace.</param>
        /// <returns>0 means no match. Higher value means more relevant match.</returns>
        public abstract int GetMatchRelevance(Namespace ns);

        /// <summary>
        /// Returns a value indicating whether this namespace specification matches a given namespace.
        /// </summary>
        /// <param name="ns">A namespace.</param>
        /// <returns>True if this namespace specification matches the given namespace.</returns>
        public bool Matches(Namespace ns) => GetMatchRelevance(ns) > 0;

        public override string ToString() => NamespaceSpecificationAsString;

        public bool Equals(NamespaceSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return String.Equals(NamespaceSpecificationAsString, other.NamespaceSpecificationAsString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamespaceSpecification)obj);
        }

        public override int GetHashCode()
        {
            return (NamespaceSpecificationAsString != null ? NamespaceSpecificationAsString.GetHashCode() : 0);
        }

        public static bool operator ==(NamespaceSpecification left, NamespaceSpecification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamespaceSpecification left, NamespaceSpecification right)
        {
            return !Equals(left, right);
        }
    }
}
