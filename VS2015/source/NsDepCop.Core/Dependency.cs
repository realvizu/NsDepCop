using System;
using System.Text;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Represents a dependency between 2 namespace specifications.
    /// The 'From' namespace specification depends on the 'To' namespace specification.
    /// A namespace specification can represent more than just a single namespace (eg. a subtree of namespaces).
    /// </summary>
    public class Dependency : IEquatable<Dependency>
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
        public Dependency(NamespaceSpecification from, NamespaceSpecification to)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(@from));

            if (to == null)
                throw new ArgumentNullException(nameof(to));
            
            From = from;
            To = to;
        }

        /// <summary>
        /// Initilaizes a new instance by converting the string parameters to NamespaceSpecification objects.
        /// </summary>
        /// <param name="from">A namespace specification in string format. The source of the dependency.</param>
        /// <param name="to">A namespace specification in string format. The target of the dependency.</param>
        public Dependency(string from, string to)
            : this(new NamespaceSpecification(from, validate: false), new NamespaceSpecification(to, validate: false))
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

        public bool Equals(Dependency other)
        {
            return Equals(From, other.From) && Equals(To, other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Dependency) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((From != null ? From.GetHashCode() : 0)*397) ^ (To != null ? To.GetHashCode() : 0);
            }
        }
    }
}
