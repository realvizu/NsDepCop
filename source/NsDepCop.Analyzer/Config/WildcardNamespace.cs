using System;
using System.Linq;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a namespace specification with wildcards, eg. 'System.IO.*' or 'System.?.?.Generic'.
    /// Each '?' can be replaced with exactly one namespace component when matching against a namespace.
    /// Each '*' can be replaced with any number of namespace components.
    /// <br/>
    /// This class is immutable.
    /// </summary>
    /// <remarks>
    /// The 'any namespace' (represented by a star '*') is also a namespace that contains every namespace.
    /// </remarks>
    [Serializable]
    public sealed class WildcardNamespace : DomainSpecification
    {
        private readonly string[] _namespaceComponents;
        public const string SingleNamespaceMarker = "?";
        public const string AnyNamespacesMarker = "*";

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="wildcardNamespaceAsString">The string representation of a namespace pattern containing wildcards.</param>
        /// <param name="validate">True means validate the input string.</param>
        public WildcardNamespace(string wildcardNamespaceAsString, bool validate = true)
            : base(wildcardNamespaceAsString, validate, IsValid)
        {
           _namespaceComponents = wildcardNamespaceAsString.Split(DomainPartSeparator);
        }

        /// <inheritdoc />
        public override int GetMatchRelevance(Namespace ns)
        {
           var actualList = ns.ToString().Split(DomainPartSeparator);

           var distance = CalcDistance(actualList, _namespaceComponents, 0);

           return int.MaxValue - distance;
        }

        /// <summary>
        /// Returns a boolean value indication if the given <paramref name="namespaceAsString"/> is a valid <see cref="WildcardNamespace"/>.
        /// </summary>
        /// <param name="namespaceAsString">The namespace string to check.</param>
        /// <returns>True, if and only if the <paramref name="namespaceAsString"/> is valid.</returns>
        public static bool IsValid(string namespaceAsString)
        {
            var parts = namespaceAsString.Split(DomainPartSeparator);

            bool validChars = parts.All(s =>
                !string.IsNullOrWhiteSpace(s)
                && (s is SingleNamespaceMarker or AnyNamespacesMarker || (!s.Contains(SingleNamespaceMarker) && !s.Contains(AnyNamespacesMarker)))
                );
            bool anyWildcard = parts.Any(s => s is SingleNamespaceMarker or AnyNamespacesMarker);
            bool notAdjacentTrees = parts.Length > 0 && parts.Zip(parts.Skip(1), (a, b) => (a, b)).All(p => p.a != AnyNamespacesMarker || p.b != AnyNamespacesMarker);

            return validChars && anyWildcard && notAdjacentTrees;
        }

        /// <summary>
        /// Calculates the edit distance between <paramref name="remainingPattern"/> and <paramref name="remainingActual"/>.
        /// <br/>
        /// The edit distance is calculated as the sum of all edit operations which are needed to replace the wildcards with
        /// the namespace names. The costs are as follows:
        /// <ul>
        /// <li>* Replacing a `?` has a cost of 1.</li>
        /// <li>* Replacing a `*` has a cost of 1 and additionaly a cost of 1 per sub-namespace that replaces the `*`.</li>
        /// </ul>
        /// </summary>
        /// <param name="remainingActual">A span of nested namespace names to match.</param>
        /// <param name="remainingPattern">A span of nested namespace names or wildcards to match with.</param>
        /// <param name="actualDistance">The edit cost of the parent namespace.</param>
        /// <returns>
        /// The sum of actualDistance and the edit distance between <paramref name="remainingActual"/> and <paramref name="remainingPattern"/>.
        /// </returns>
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
                    // try both options: (This moves the algorithm to complexity class O(NM).)
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
