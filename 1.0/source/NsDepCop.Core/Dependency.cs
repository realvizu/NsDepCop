using System;
using System.Diagnostics;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Represents a dependency between 2 namespace specifications.
    /// The 'From' namespace specification depends on the 'To' namespace specification.
    /// A namespace specification can represent more than just a single namespace (eg. a subtree of namespaces).
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// The dependency points from this namespace to the other.
        /// </summary>
        public NamespaceSpecification From { get; private set; }

        /// <summary>
        /// The dependency points into this namespace.
        /// </summary>
        public NamespaceSpecification To { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="from">The starting point of the dependency.</param>
        /// <param name="to">The starting point of the dependency.</param>
        public Dependency(NamespaceSpecification from, NamespaceSpecification to)
        {
            if (from == null)
                throw new ArgumentNullException("from");

            if (to == null)
                throw new ArgumentNullException("to");
            
            From = from;
            To = to;
        }

        /// <summary>
        /// Initilaizes a new instance by converting the string parameters to NamespaceSpecification objects.
        /// </summary>
        /// <param name="from">A namespace specification in string format. The starting point of the dependency.</param>
        /// <param name="to">A namespace specification in string format. The starting point of the dependency.</param>
        public Dependency(string from, string to)
            : this(new NamespaceSpecification(from), new NamespaceSpecification(to))
        { }

        /// <summary>
        /// Returns the string represenation of a namespace dependency.
        /// </summary>
        /// <returns>The string represenation of a namespace dependency.</returns>
        public override string ToString()
        {
            return string.Format("{0}->{1}", From, To);
        }
    }
}
