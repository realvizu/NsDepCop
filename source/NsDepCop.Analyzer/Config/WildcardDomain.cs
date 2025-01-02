using System;
using System.Linq;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a domain specification with wildcards, eg. 'System.IO.*' or 'System.?.?.Generic'.
    /// Each '?' can be replaced with exactly one domain component when matching against a domain.
    /// Each '*' can be replaced with any number of domain components.
    /// <br/>
    /// This class is immutable.
    /// </summary>
    /// <remarks>
    /// The 'any domain' (represented by a star '*') is also a domain that contains every domain.
    /// </remarks>
    [Serializable]
    public sealed class WildcardDomain : DomainSpecification
    {
        private readonly string[] _domainComponents;
        public const string SingleDomainMarker = "?";
        public const string AnyDomainMarker = "*";

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="wildcardDomainAsString">The string representation of a domain pattern containing wildcards.</param>
        /// <param name="validate">True means validate the input string.</param>
        public WildcardDomain(string wildcardDomainAsString, bool validate = true)
            : base(wildcardDomainAsString, validate, IsValid)
        {
            _domainComponents = wildcardDomainAsString.Split(DomainPartSeparator);
        }

        /// <inheritdoc />
        public override int GetMatchRelevance(Namespace ns)
        {
            var actualList = ns.ToString().Split(DomainPartSeparator);

            var distance = CalcDistance(actualList, _domainComponents, 0);

            return int.MaxValue - distance;
        }

        /// <summary>
        /// Returns a boolean value indication if the given <paramref name="domainAsString"/> is a valid <see cref="WildcardDomain"/>.
        /// </summary>
        /// <param name="domainAsString">The domain string to check.</param>
        /// <returns>True, if and only if the <paramref name="domainAsString"/> is valid.</returns>
        public static bool IsValid(string domainAsString)
        {
            var parts = domainAsString.Split(DomainPartSeparator);

            bool validChars = parts.All(s =>
                !string.IsNullOrWhiteSpace(s)
                && (s is SingleDomainMarker or AnyDomainMarker || (!s.Contains(SingleDomainMarker) && !s.Contains(AnyDomainMarker)))
                );
            bool anyWildcard = parts.Any(s => s is SingleDomainMarker or AnyDomainMarker);
            bool notAdjacentTrees = parts.Length > 0 && parts.Zip(parts.Skip(1), (a, b) => (a, b)).All(p => p.a != AnyDomainMarker || p.b != AnyDomainMarker);

            return validChars && anyWildcard && notAdjacentTrees;
        }

        /// <summary>
        /// Calculates the edit distance between <paramref name="remainingPattern"/> and <paramref name="remainingActual"/>.
        /// <br/>
        /// The edit distance is calculated as the sum of all edit operations which are needed to replace the wildcards with
        /// the domain names. The costs are as follows:
        /// <ul>
        /// <li>* Replacing a `?` has a cost of 1.</li>
        /// <li>* Replacing a `*` has a cost of 1 and additionally a cost of 1 per sub-domain that replaces the `*`.</li>
        /// </ul>
        /// </summary>
        /// <param name="remainingActual">A span of nested domain names to match.</param>
        /// <param name="remainingPattern">A span of nested domain names or wildcards to match with.</param>
        /// <param name="actualDistance">The edit cost of the parent domain.</param>
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
                if (remainingPattern.Length == 1 && remainingPattern[0] is AnyDomainMarker)
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
                case SingleDomainMarker:
                    {
                        // replace single wildcard at cost 1
                        return CalcDistance(remainingActual.Slice(1), remainingPattern.Slice(1), actualDistance + 1);
                    }

                case AnyDomainMarker:
                    {
                        // try both options: (This moves the algorithm to complexity class O(NM).)
                        // remove wildcard
                        int v1 = CalcDistance(remainingActual, remainingPattern.Slice(1), actualDistance + 1);

                        // use wildcard to substitute one domain component
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
