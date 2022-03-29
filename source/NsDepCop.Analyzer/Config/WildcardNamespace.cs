using System;
using System.Linq;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a namespace specification with wildcards, eg. 'System.IO.*' or 'System.*.Generic'. Immutable.
    /// </summary>
    /// <remarks>
    /// The 'any namespace' (represented by a star '*') is also a namespace that contains every namespace.
    /// </remarks>
    [Serializable]
    public sealed class WildcardNamespace : NamespaceSpecification
    {
        private readonly string[] namespaceComponents;
        public const string SingleNamespaceMarker = "?";
        public const string AnyNamespacesMarker = "*";

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="wildcardNamespaceAsString">The string representation of a namespace tree.</param>
        /// <param name="validate">True means validate the input string.</param>
        public WildcardNamespace(string wildcardNamespaceAsString, bool validate = true)
            : base(wildcardNamespaceAsString, validate, IsValid)
        {
           namespaceComponents = wildcardNamespaceAsString.Split(NamespacePartSeparator);
        }

        public override int GetMatchRelevance(Namespace ns)
        {
           var actualList = ns.ToString().Split(NamespacePartSeparator);

           var distance = CalcDistance(actualList, namespaceComponents, 0);

           return int.MaxValue - distance;
        }

        public static bool IsValid(string namespaceAsString)
        {
            var parts = namespaceAsString.Split(NamespacePartSeparator);

            bool validChars = parts.All(s =>
                !string.IsNullOrWhiteSpace(s)
                && (s is SingleNamespaceMarker or AnyNamespacesMarker || (!s.Contains(SingleNamespaceMarker) && !s.Contains(AnyNamespacesMarker)))
                );
            bool anyWildcard = parts.Any(s => s is SingleNamespaceMarker or AnyNamespacesMarker);
            bool notAdjacentTrees = parts.Length > 0 && parts.Zip(parts.Skip(1), (a, b) => (a, b)).All(p => p.a != AnyNamespacesMarker || p.b != AnyNamespacesMarker);

            return validChars && anyWildcard && notAdjacentTrees;
        }

        private static int CalcDistance(ReadOnlySpan<string> remainingActual, ReadOnlySpan<string> remainingPattern, int actualDistance)
        {
            if (remainingPattern.IsEmpty)
            {
                if (remainingActual.IsEmpty)
                {
                    // completely matched
                    return actualDistance;
                }

                // no match, as the pattern is exhausted
                return int.MaxValue;
            }

            if (remainingActual.IsEmpty)
            {
                if (remainingPattern.Length == 1 && remainingPattern[0] is AnyNamespacesMarker)
                {
                    // removing the trailing star costs one point
                    return actualDistance + 1;
                }

                // no match, as no more input can be matched against the pattern
                return int.MaxValue;
            }

            System.Diagnostics.Debug.Assert(!remainingActual.IsEmpty && !remainingPattern.IsEmpty);
            switch (remainingPattern[0])
            {
                case SingleNamespaceMarker:
                {
                    // replace single wildcard at cost 1
                    return CalcDistance(remainingActual.Slice(1), remainingPattern.Slice(1), actualDistance + 1);
                }

                case AnyNamespacesMarker:
                {
                    // try both options: (This moves the algorithm to class O(NM).)
                    // remove wildcard
                    int v1 = CalcDistance(remainingActual, remainingPattern.Slice(1), actualDistance + 1);

                    // use wildcard to substitute one namespace component
                    int v2 = CalcDistance(remainingActual.Slice(1), remainingPattern, actualDistance + 1);

                    return Math.Min(v1, v2);
                }

                default:
                {
                    if (remainingPattern[0] == remainingActual[0])
                    {
                        // full match
                        return CalcDistance(remainingActual.Slice(1), remainingPattern.Slice(1), actualDistance);
                    }

                    // no match
                    return int.MaxValue;
                }

            }
        }
    }
}
