using System;
using System.Linq;

namespace Codartis.NsDepCop.Interface.Config
{
    /// <summary>
    /// Represents a namespace, eg. 'A.B'. Immutable.
    /// </summary>
    /// <remarks>
    /// The global namespace is also a namespace and it's represented by '.' (a dot)
    /// </remarks>
    [Serializable]
    public sealed class Namespace : NamespaceSpecification
    {
        public const string RootNamespaceMarker = ".";

        /// <summary>
        /// Represents the global namespace.
        /// </summary>
        public static readonly Namespace GlobalNamespace = new Namespace(RootNamespaceMarker);

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="namespaceAsString">The string representation of a namespace.</param>
        /// <param name="validate">True means validate the input string.</param>
        public Namespace(string namespaceAsString, bool validate = true)
            : base(Normalize(namespaceAsString), validate, IsValid)
        {
        }

        public override int GetMatchRelevance(Namespace ns)
        {
            return this == ns
                ? int.MaxValue
                : 0;
        }

        /// <summary>
        /// Determines whether this namespace is a subnamespace of the given other one.
        /// </summary>
        /// <param name="parentCandidate">The namespace to test whether it's a parent of the current namespace.</param>
        /// <returns>True if this namespace is a subnamespace of the given one, false otherwise.</returns>
        public bool IsSubnamespaceOf(Namespace parentCandidate)
        {
            if (this == GlobalNamespace)
                return false;

            if (parentCandidate == GlobalNamespace)
                return true;

            var parentPrefix = parentCandidate.NamespaceSpecificationAsString + NamespacePartSeparator;
            return NamespaceSpecificationAsString.StartsWith(parentPrefix);
        }

        /// <summary>
        /// Validates that the given string represents a valid namespace specification.
        /// </summary>
        /// <param name="namespaceAsString">A namespace specification in string format.</param>
        /// <returns>True if the given string represents a valid namespace specification.</returns>
        public static bool IsValid(string namespaceAsString)
        {
            if (namespaceAsString == RootNamespaceMarker)
                return true;

            if (namespaceAsString.EndsWith(NamespaceTree.AnyNamespaceMarker))
                return false;

            var pieces = namespaceAsString.Split(new[] { NamespacePartSeparator }, StringSplitOptions.None);

            return pieces.All(i => !string.IsNullOrWhiteSpace(i));
        }

        /// <summary>
        /// Converts the input string into a standard representation (removes ambiguities).
        /// </summary>
        /// <param name="namespaceAsString">A namespace specification in string format.</param>
        /// <returns>The standard representation of the given namespace specification string.</returns>
        private static string Normalize(string namespaceAsString)
        {
            // Global namespace representations:
            //   Roslyn: "<global namespace>"
            //   NRefactory: "" (empty string)
            //   NsDepCop: "." (dot)
            if (namespaceAsString == "" ||
                namespaceAsString == "<global namespace>")
                namespaceAsString = RootNamespaceMarker;

            return namespaceAsString;
        }
    }
}
