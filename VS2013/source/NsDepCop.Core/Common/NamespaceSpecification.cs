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
    /// <item>Any namespaces: '*'</item>
    /// </list>
    /// </remarks>
    public class NamespaceSpecification : IEquatable<NamespaceSpecification>
    {
        /// <summary>
        /// Represents the global namespace.
        /// </summary>
        public static readonly NamespaceSpecification GlobalNamespace = new NamespaceSpecification(".", validate: false);

        /// <summary>
        /// Represents any namespace.
        /// </summary>
        public static readonly NamespaceSpecification AnyNamespace = new NamespaceSpecification("*", validate: false);

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
                namespaceSpecificationAsString = ".";

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
            if (namespaceSpecification == "." || namespaceSpecification == "*")
                return true;

            var pieces = namespaceSpecification.Split(new[] { '.' }, StringSplitOptions.None);
            for (var i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];

                if (string.IsNullOrWhiteSpace(piece))
                    return false;

                // Only the last piece can be '*' (any namespace)
                if (i < pieces.Length - 1 && piece == "*")
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns all the different namespace specifications that can contain the given namespace.
        /// </summary>
        /// <param name="namespaceName">A concrete namespace name in string format.</param>
        /// <returns>A collection of all NamespaceSpecifications that can contain the given namespace.</returns>
        public static IEnumerable<NamespaceSpecification> GetContainingNamespaceSpecifications(string namespaceName)
        {
            // Convert the string to namespace specification, also validates it.
            var namespaceSpecification = new NamespaceSpecification(namespaceName, validate: false);

            // The AnyNamespace specification contains every namespace.
            yield return AnyNamespace;

            // The namespace specification created from the given namespace obviously contains the given namespace.
            yield return namespaceSpecification;

            // For the global namespace there's no more containing namespace.
            if (namespaceSpecification == GlobalNamespace)
                yield break;

            // For any other namespace return itself and all parent namespaces postfixed with '*' (meaning any sub-namespaces).
            // Eg. for 'System.Collections.Generic'
            // Return 'System.*'
            // Return 'System.Collections.*'
            // Return 'System.Collections.Generic.*'

            var prefix = "";
            foreach (var piece in namespaceName.Split('.'))
            {
                if (prefix.Length > 0)
                    prefix += ".";

                prefix += piece;
                yield return new NamespaceSpecification(prefix + ".*", validate: false);
            }
        }
    }
}
