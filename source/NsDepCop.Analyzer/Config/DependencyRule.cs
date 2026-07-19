using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a dependency rule between two domain specifications. Immutable.
    /// </summary>
    /// <remarks>
    /// The 'From' domain specification depends on the 'To' domain specification.
    /// A domain specification can represent more than just a single domain (eg. a subtree of namespaces).
    /// </remarks>
    [Serializable]
    public class DependencyRule
    {
        /// <summary>
        /// Caches the domain specifications created by substituting captured placeholder values
        /// into the 'To' side, keyed by the substituted string.
        /// Not serialized; recreated on demand after deserialization.
        /// </summary>
        [NonSerialized] private ConcurrentDictionary<string, DomainSpecification> _substitutedToSpecificationCache;

        /// <summary>
        /// The dependency points from this domain to the other.
        /// </summary>
        public DomainSpecification From { get; }

        /// <summary>
        /// The dependency points into this domain.
        /// </summary>
        public DomainSpecification To { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="from">The source of the dependency.</param>
        /// <param name="to">The target of the dependency.</param>
        public DependencyRule(DomainSpecification from, DomainSpecification to)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            To = to ?? throw new ArgumentNullException(nameof(to));

            ValidatePlaceholderUsage(From, To);
        }

        /// <summary>
        /// Initializes a new instance by converting the string parameters to NamespaceSpecification objects.
        /// </summary>
        /// <param name="from">A namespace specification in string format. The source of the dependency.</param>
        /// <param name="to">A namespace specification in string format. The target of the dependency.</param>
        public DependencyRule(string from, string to)
            : this(DomainSpecificationParser.Parse(from), DomainSpecificationParser.Parse(to))
        { }

        /// <summary>
        /// Returns a value indicating whether this rule matches the given (from, to) domain pair.
        /// </summary>
        /// <remarks>
        /// For rules without placeholders this is equivalent to matching both sides independently.
        /// If both sides are <see cref="PlaceholderDomain"/>s, the values captured on the 'From' side
        /// are applied to the 'To' side before matching it, which links the two sides: positive
        /// placeholders are substituted with the captured value, negated placeholders must differ from it.
        /// </remarks>
        /// <param name="from">The source domain of the dependency.</param>
        /// <param name="to">The target domain of the dependency.</param>
        /// <returns>True if this rule matches the given domain pair.</returns>
        public virtual bool Matches(Domain from, Domain to)
        {
            if (From is PlaceholderDomain fromPlaceholder)
            {
                // Always match the 'From' side with placeholder semantics ('[Name*]' captures one or
                // more components), never the wildcard-degraded relevance (where '*' also matches zero).
                // This keeps a placeholder's meaning independent of what is on the 'To' side.
                if (!fromPlaceholder.TryMatch(from, out var capturedValues))
                    return false;

                // If the 'To' side has placeholders too, link the two sides via the captured values.
                // (A 'To'-side placeholder is only reachable when the 'From' side has one, so this
                // is the only place the link can occur.)
                if (To is PlaceholderDomain toPlaceholder)
                {
                    // Negated placeholders ('[!Name]') have no substitution semantics,
                    // so the 'To' side is matched directly against the captured bindings.
                    return toPlaceholder.HasNegatedPlaceholders
                        ? toPlaceholder.Matches(to, capturedValues)
                        : GetSubstitutedToSpecification(toPlaceholder, capturedValues).Matches(to);
                }

                // Placeholder only on the 'From' side: the captured values are unused; match 'To' independently.
                return To.Matches(to);
            }

            return From.Matches(from) && To.Matches(to);
        }

        /// <summary>
        /// Returns a number indicating how well the 'From' side of this rule matches the given domain.
        /// Used to select the most specific rule among all matching rules.
        /// </summary>
        /// <param name="from">The source domain of the dependency.</param>
        /// <returns>Zero means no match. Higher value means more relevant match.</returns>
        public virtual int GetFromMatchRelevance(Domain from) => From.GetMatchRelevance(from);

        private DomainSpecification GetSubstitutedToSpecification(
            PlaceholderDomain toPlaceholder,
            IReadOnlyDictionary<string, string> capturedValues)
        {
            var substituted = toPlaceholder.Substitute(capturedValues);

            var cache = LazyInitializer.EnsureInitialized(ref _substitutedToSpecificationCache);

            // The substituted string contains no placeholders anymore, so the parser
            // yields a Domain or (if '?'/'*' wildcards remain) a WildcardDomain.
            return cache.GetOrAdd(substituted, static s => DomainSpecificationParser.Parse(s));
        }

        private static void ValidatePlaceholderUsage(DomainSpecification from, DomainSpecification to)
        {
            var fromTokens = (from as PlaceholderDomain)?.PlaceholderTokens.ToList()
                             ?? new List<(string Name, bool IsMulti, bool IsNegated)>();

            if (fromTokens.Any(t => t.IsNegated))
                throw new FormatException($"Negated placeholders are not allowed on the 'From' side ('{from}').");

            var fromPlaceholderNames = fromTokens.Select(t => t.Name).ToList();

            if (fromPlaceholderNames.Count != fromPlaceholderNames.Distinct().Count())
                throw new FormatException($"Placeholder names in '{from}' must be unique.");

            if (to is PlaceholderDomain toPlaceholder)
            {
                var toTokens = toPlaceholder.PlaceholderTokens.ToList();

                var unboundNames = toTokens.Select(t => t.Name).Distinct().Except(fromPlaceholderNames).ToList();
                if (unboundNames.Count > 0)
                    throw new FormatException(
                        $"Placeholder(s) {string.Join(", ", unboundNames)} in '{to}' are not bound by '{from}'.");

                // A negated reference compares exactly one component, so it must refer
                // to a single-component capture ('[Name]'), not a multi capture ('[Name*]').
                var fromMultiNames = fromTokens.Where(t => t.IsMulti).Select(t => t.Name).ToList();
                var invalidNegatedNames = toTokens
                    .Where(t => t.IsNegated && fromMultiNames.Contains(t.Name))
                    .Select(t => t.Name)
                    .Distinct()
                    .ToList();

                if (invalidNegatedNames.Count > 0)
                    throw new FormatException(
                        $"Negated placeholder(s) {string.Join(", ", invalidNegatedNames)} in '{to}' must not refer to multi placeholders in '{from}'.");
            }
        }

        /// <summary>
        /// Returns the string representation of a namespace dependency.
        /// </summary>
        /// <returns>The string representation of a namespace dependency.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(From);
            builder.Append("->");
            builder.Append(To);
            return builder.ToString();
        }

        public bool Equals(DependencyRule other)
        {
            return Equals(From, other.From) && Equals(To, other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DependencyRule)obj);
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
