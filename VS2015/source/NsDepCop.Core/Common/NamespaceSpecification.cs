using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Represents a namespace or a namespace tree.
    /// </summary>
    /// <remarks>
    /// Valid namespace specifications are:
    /// <list type="bullet">
    /// <item>A concrete namespace, eg. 'System.IO'</item>
    /// <item>A concrete namespace and all subnamespaces, eg. 'System.IO.*'</item>
    /// <item>The global namespaces: '.'</item>
    /// <item>Any namespace: '*'</item>
    /// </list>
    /// </remarks>
    public class NamespaceSpecification : IEquatable<NamespaceSpecification>
    {
        private const string ROOT_NAMESPACE_MARKER = ".";
        private const string ANY_NAMESPACE_MARKER = "*";
        private const char NAMESPACE_PART_SEPARATOR = '.';

        /// <summary>
        /// Represents the global namespace.
        /// </summary>
        public static readonly NamespaceSpecification GlobalNamespace = new NamespaceSpecification(ROOT_NAMESPACE_MARKER, validate: false);

        /// <summary>
        /// Represents any namespace.
        /// </summary>
        public static readonly NamespaceSpecification AnyNamespace = new NamespaceSpecification(ANY_NAMESPACE_MARKER, validate: false);

        /// <summary>
        /// The namespace specification stored as a string.
        /// </summary>
        private readonly string _namespaceSpecificationAsString;

        /// <summary>
        /// Initializes a new instance. Also validates the input format if needed.
        /// </summary>
        /// <param name="namespaceSpecificationAsString">The string representation of the namespace specification.</param>
        /// <param name="validate">True means input format validation.</param>
        public NamespaceSpecification(string namespaceSpecificationAsString, bool validate = true)
        {
            if (namespaceSpecificationAsString == null)
                throw new ArgumentNullException();

            // Global namespace representations:
            //   Roslyn: "<global namespace>"
            //   NRefactory: "" (empty string)
            //   NsDepCop: "." (dot)
            if (namespaceSpecificationAsString == "" || 
                namespaceSpecificationAsString == "<global namespace>")
                namespaceSpecificationAsString = ROOT_NAMESPACE_MARKER;

            if (validate && !IsValidNamespaceSpecification(namespaceSpecificationAsString))
                throw new FormatException("Not a valid namespace specification.");

            _namespaceSpecificationAsString = namespaceSpecificationAsString;
        }

        public override string ToString()
        {
            return _namespaceSpecificationAsString;
        }

        public bool Equals(NamespaceSpecification other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_namespaceSpecificationAsString, other._namespaceSpecificationAsString);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamespaceSpecification) obj);
        }

        public override int GetHashCode()
        {
            return (_namespaceSpecificationAsString != null ? _namespaceSpecificationAsString.GetHashCode() : 0);
        }

        public static bool operator ==(NamespaceSpecification left, NamespaceSpecification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamespaceSpecification left, NamespaceSpecification right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Validates that a string represents a namespace specification.
        /// </summary>
        /// <param name="namespaceSpecification">A namespace specification in string format.</param>
        /// <returns>True if the parameter is a namespace specification. False otherwise.</returns>
        private static bool IsValidNamespaceSpecification(string namespaceSpecification)
        {
            // "." means the global namespace, '*' means any namespace.
            if (namespaceSpecification == ROOT_NAMESPACE_MARKER || namespaceSpecification == ANY_NAMESPACE_MARKER)
                return true;

            var pieces = namespaceSpecification.Split(new[] { ROOT_NAMESPACE_MARKER }, StringSplitOptions.None);
            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];

                if (string.IsNullOrWhiteSpace(piece))
                    return false;

                // Only the last piece can be '*' (any namespace)
                if (i < pieces.Length - 1 && piece == ANY_NAMESPACE_MARKER)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether this namespace is a subnamespace of the given other one.
        /// </summary>
        /// <param name="parentCandidate"></param>
        /// <returns></returns>
        public bool IsSubnamespaceOf(NamespaceSpecification parentCandidate)
        {
            if (this == GlobalNamespace 
                || this == AnyNamespace
                || _namespaceSpecificationAsString.EndsWith(ANY_NAMESPACE_MARKER)
                || parentCandidate.ToString().EndsWith(ANY_NAMESPACE_MARKER))
                return false;

            if (parentCandidate == GlobalNamespace)
                return true;

            return _namespaceSpecificationAsString.StartsWith(parentCandidate.ToString() + NAMESPACE_PART_SEPARATOR);
        }

        /// <summary>
        /// Returns all the different namespace specifications that can contain this namespace.
        /// </summary>
        /// <returns>A collection of all NamespaceSpecifications that can contain this namespace.</returns>
        public IEnumerable<NamespaceSpecification> GetContainingNamespaceSpecifications()
        {
            // The AnyNamespace specification contains every namespace.
            yield return AnyNamespace;

            // The namespace specification created from the given namespace obviously contains the given namespace.
            yield return this;

            // For the global namespace there's no more containing namespace.
            if (this == GlobalNamespace)
                yield break;

            // For any other namespace return itself and all parent namespaces postfixed with '*' (meaning any sub-namespaces).
            // Eg. for 'System.Collections.Generic'
            // Return 'System.*'
            // Return 'System.Collections.*'
            // Return 'System.Collections.Generic.*'

            var prefix = "";
            foreach (var piece in _namespaceSpecificationAsString.Split(NAMESPACE_PART_SEPARATOR))
            {
                if (prefix.Length > 0)
                    prefix += NAMESPACE_PART_SEPARATOR;

                prefix += piece;
                yield return new NamespaceSpecification(prefix + NAMESPACE_PART_SEPARATOR + ANY_NAMESPACE_MARKER, validate: false);
            }
        }
    }
}
