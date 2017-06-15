using System;
using System.Text;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Represents a dependency rule between 2 namespace specifications.
    /// The 'From' namespace specification depends on the 'To' namespace specification.
    /// A namespace specification can represent more than just a single namespace (eg. a subtree of namespaces).
    /// </summary>
    public class NamespaceDependencyRule
    {
        /// <summary>
        /// The dependency points from this namespace to the other.
        /// </summary>
        public NamespaceSpecification From { get; }

        /// <summary>
        /// The dependency points into this namespace.
        /// </summary>
        public NamespaceSpecification To { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="from">The source of the dependency.</param>
        /// <param name="to">The target of the dependency.</param>
        public NamespaceDependencyRule(NamespaceSpecification from, NamespaceSpecification to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));
        }

        /// <summary>
        /// Initilaizes a new instance by converting the string parameters to NamespaceSpecification objects.
        /// </summary>
        /// <param name="from">A namespace specification in string format. The source of the dependency.</param>
        /// <param name="to">A namespace specification in string format. The target of the dependency.</param>
        public NamespaceDependencyRule(string from, string to)
            : this(NamespaceSpecificationParser.Parse(from), NamespaceSpecificationParser.Parse(to))
        { }

        /// <summary>
        /// Returns the string represenation of a namespace dependency.
        /// </summary>
        /// <returns>The string represenation of a namespace dependency.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(From);
            builder.Append("->");
            builder.Append(To);
            return builder.ToString();
        }

        public bool Equals(NamespaceDependencyRule other)
        {
            return Equals(From, other.From) && Equals(To, other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamespaceDependencyRule)obj);
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
