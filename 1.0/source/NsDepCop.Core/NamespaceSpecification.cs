using System;
using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Represents a namespace or a set of namespaces.
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
    public class NamespaceSpecification
    {
        /// <summary>
        /// Represents the global namespace.
        /// </summary>
        public static NamespaceSpecification GlobalNamespace = new NamespaceSpecification(".");

        /// <summary>
        /// Represents any namespace.
        /// </summary>
        public static NamespaceSpecification AnyNamespace = new NamespaceSpecification("*");

        /// <summary>
        /// The namespace specification stored as a string.
        /// </summary>
        private string _namespaceSpecificationAsString;

        /// <summary>
        /// Initializes a new instance. Validates the input format.
        /// </summary>
        /// <param name="namespaceSpecificationAsString">The string representation of the namespace specification.</param>
        public NamespaceSpecification(string namespaceSpecificationAsString)
        {
            if (namespaceSpecificationAsString == null)
                throw new ArgumentNullException("namespaceSpecificationAsString");

            // Roslyn represents the global namespace like this: "<global namespace>".
            // This tool represents it like this: "."
            if (namespaceSpecificationAsString == "<global namespace>")
                namespaceSpecificationAsString = ".";

            if (!IsValidNamespaceSpecification(namespaceSpecificationAsString))
                throw new ArgumentException("Not a valid namespace specification.");

            _namespaceSpecificationAsString = namespaceSpecificationAsString;
        }

        /// <summary>
        /// Returns the string representation of the namespace specification.
        /// </summary>
        /// <returns>The string representation of the namespace specification.</returns>
        public override string ToString()
        {
            return _namespaceSpecificationAsString;
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
            return pieces.All(i => !string.IsNullOrWhiteSpace(i));
        }

        /// <summary>
        /// Returns all the different namespace specifications that can contain the given namespace.
        /// </summary>
        /// <param name="namespaceName">A concrete namespace name in string format.</param>
        /// <returns>All that string that can produce a match when searching matching rules.</returns>
        public static IEnumerable<NamespaceSpecification> GetContainingNamespaceSpecifications(string namespaceName)
        {
            // The AnyNamespace specification contains every namespace.
            yield return AnyNamespace;

            // The namespace specification created from the given namespace obviously contains the given namespace.
            yield return new NamespaceSpecification(namespaceName);

            // And return all the containing namespaces with a '*' (meaning sub-namespaces).
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
                yield return new NamespaceSpecification(prefix + ".*");
            }
        }
    }
}
