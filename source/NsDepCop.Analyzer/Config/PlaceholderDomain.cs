using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a domain specification with named placeholders, eg. 'MyApp.[Module].Domain'.
    /// <br/>
    /// A placeholder used on the 'From' side of a dependency rule captures the matched domain component(s);
    /// the same placeholder on the 'To' side is substituted with the captured value before matching.
    /// <ul>
    /// <li>'[Name]' matches and captures exactly one domain component (like '?').</li>
    /// <li>'[Name*]' matches and captures one or more domain components (like '*', but at least one).</li>
    /// <li>'[!Name]' (only valid on the 'To' side) matches exactly one component that is NOT equal
    /// to the value captured for 'Name' on the 'From' side.</li>
    /// <li>Regular '?' and '*' wildcards may be mixed in; they match but do not capture.</li>
    /// </ul>
    /// When several capture lengths are possible (eg. '[A*].[B]'), the leftmost placeholder
    /// captures as few components as possible (shortest-first, deterministic).
    /// <br/>
    /// This class is immutable.
    /// </summary>
    [Serializable]
    public sealed class PlaceholderDomain : DomainSpecification
    {
        public const string PlaceholderStartMarker = "[";
        public const string PlaceholderEndMarker = "]";
        public const string PlaceholderNegationMarker = "!";

        private static readonly Regex PlaceholderTokenRegex =
            new Regex(@"^\[(?<negated>!)?(?<name>[A-Za-z_][A-Za-z0-9_]*)(?<multi>\*)?\]$", RegexOptions.Compiled);

        private readonly string[] _components;

        private enum TokenKind
        {
            Literal,
            SingleWildcard,
            AnyWildcard,
            Placeholder
        }

        /// <summary>
        /// A pre-parsed pattern component, so that the match path needs no Regex evaluation.
        /// </summary>
        private readonly struct Token
        {
            public readonly TokenKind Kind;
            public readonly string Text;
            public readonly string Name;
            public readonly bool IsMulti;
            public readonly bool IsNegated;

            public Token(TokenKind kind, string text, string name, bool isMulti, bool isNegated)
            {
                Kind = kind;
                Text = text;
                Name = name;
                IsMulti = isMulti;
                IsNegated = isNegated;
            }
        }

        /// <summary>
        /// The pre-parsed pattern components.
        /// Not serialized; recreated on demand after deserialization.
        /// </summary>
        [NonSerialized] private Token[] _tokens;

        /// <summary>
        /// Lazily created equivalent where placeholders are degraded to plain wildcards
        /// ('[Name]' -> '?', '[Name*]' -> '*'). Used for match relevance calculation.
        /// Not serialized; recreated on demand after deserialization.
        /// </summary>
        [NonSerialized] private WildcardDomain _degradedWildcardDomain;

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="placeholderDomainAsString">The string representation of a domain pattern containing placeholders.</param>
        /// <param name="validate">True means validate the input string.</param>
        public PlaceholderDomain(string placeholderDomainAsString, bool validate = true)
            : base(placeholderDomainAsString, validate, IsValid)
        {
            _components = placeholderDomainAsString.Split(DomainPartSeparator);
            _tokens = ParseTokens(_components);
        }

        private Token[] GetTokens() => _tokens ??= ParseTokens(_components);

        private static Token[] ParseTokens(string[] components)
        {
            var tokens = new Token[components.Length];

            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component == WildcardDomain.SingleDomainMarker)
                    tokens[i] = new Token(TokenKind.SingleWildcard, component, null, false, false);
                else if (component == WildcardDomain.AnyDomainMarker)
                    tokens[i] = new Token(TokenKind.AnyWildcard, component, null, false, false);
                else if (TryParsePlaceholderToken(component, out var name, out var isMulti, out var isNegated))
                    tokens[i] = new Token(TokenKind.Placeholder, component, name, isMulti, isNegated);
                else
                    tokens[i] = new Token(TokenKind.Literal, component, null, false, false);
            }

            return tokens;
        }

        /// <summary>
        /// All placeholder tokens in order of appearance. Names may appear multiple times
        /// (allowed on the 'To' side, rejected on the 'From' side by <see cref="DependencyRule"/>).
        /// </summary>
        public IEnumerable<(string Name, bool IsMulti, bool IsNegated)> PlaceholderTokens
        {
            get
            {
                foreach (var token in GetTokens())
                {
                    if (token.Kind == TokenKind.Placeholder)
                        yield return (token.Name, token.IsMulti, token.IsNegated);
                }
            }
        }

        /// <summary>
        /// The names of all placeholders (including negated references) in order of appearance.
        /// </summary>
        public IEnumerable<string> PlaceholderNames
        {
            get
            {
                foreach (var token in PlaceholderTokens)
                    yield return token.Name;
            }
        }

        /// <summary>
        /// True if this specification contains at least one negated placeholder ('[!Name]').
        /// </summary>
        public bool HasNegatedPlaceholders
        {
            get
            {
                foreach (var token in PlaceholderTokens)
                {
                    if (token.IsNegated)
                        return true;
                }

                return false;
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Relevance is defined as the relevance of the wildcard-degraded equivalent,
        /// so 'A.[M].B' is exactly as relevant as 'A.?.B'.
        /// </remarks>
        public override int GetMatchRelevance(Domain domain)
        {
            return GetDegradedWildcardDomain().GetMatchRelevance(domain);
        }

        /// <summary>
        /// Matches this specification against a concrete domain and extracts the placeholder captures.
        /// </summary>
        /// <param name="domain">A concrete domain.</param>
        /// <param name="capturedValues">On success: placeholder name -> captured domain component(s), dot-separated for multi captures.</param>
        /// <returns>True if the domain matches this specification.</returns>
        public bool TryMatch(Domain domain, out IReadOnlyDictionary<string, string> capturedValues)
        {
            var actualComponents = domain.ToString().Split(DomainPartSeparator);
            var captures = new Dictionary<string, string>();

            if (TryMatchRecursive(actualComponents, GetTokens(), captures, boundValues: null))
            {
                capturedValues = captures;
                return true;
            }

            capturedValues = null;
            return false;
        }

        /// <summary>
        /// Matches this specification against a concrete domain using values that were captured
        /// on the 'From' side of a dependency rule. Placeholders compare against their bound value
        /// ('[Name]' must equal it, '[!Name]' must differ from it); nothing is captured.
        /// </summary>
        /// <param name="domain">A concrete domain.</param>
        /// <param name="boundValues">Placeholder name -> value captured on the 'From' side.</param>
        /// <returns>True if the domain matches this specification under the given bindings.</returns>
        public bool Matches(Domain domain, IReadOnlyDictionary<string, string> boundValues)
        {
            if (boundValues == null)
                throw new ArgumentNullException(nameof(boundValues));

            var actualComponents = domain.ToString().Split(DomainPartSeparator);

            return TryMatchRecursive(actualComponents, GetTokens(), captures: null, boundValues);
        }

        /// <summary>
        /// Replaces every placeholder with its captured value and returns the resulting
        /// domain specification string. The result may still contain '?' and '*' wildcards.
        /// </summary>
        /// <param name="capturedValues">Placeholder name -> captured value.</param>
        /// <returns>The substituted domain specification string.</returns>
        public string Substitute(IReadOnlyDictionary<string, string> capturedValues)
        {
            if (capturedValues == null)
                throw new ArgumentNullException(nameof(capturedValues));

            var resultComponents = new string[_components.Length];

            var tokens = GetTokens();

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.Kind == TokenKind.Placeholder)
                {
                    if (token.IsNegated)
                        throw new InvalidOperationException(
                            $"Negated placeholder '[{PlaceholderNegationMarker}{token.Name}]' in '{Value}' cannot be substituted; use bound matching instead.");

                    if (!capturedValues.TryGetValue(token.Name, out var capturedValue))
                        throw new InvalidOperationException($"No captured value for placeholder '{token.Name}' in '{Value}'.");

                    resultComponents[i] = capturedValue;
                }
                else
                {
                    resultComponents[i] = token.Text;
                }
            }

            return string.Join(DomainPartSeparator.ToString(), resultComponents);
        }

        /// <summary>
        /// Returns a boolean value indicating if the given <paramref name="domainAsString"/> is a valid <see cref="PlaceholderDomain"/>.
        /// </summary>
        /// <param name="domainAsString">The domain string to check.</param>
        /// <returns>True, if and only if the <paramref name="domainAsString"/> is valid.</returns>
        public static bool IsValid(string domainAsString)
        {
            var parts = domainAsString.Split(DomainPartSeparator);

            var anyPlaceholder = false;

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    return false;

                if (TryParsePlaceholderToken(part, out _, out _, out _))
                {
                    anyPlaceholder = true;
                    continue;
                }

                if (part is WildcardDomain.SingleDomainMarker or WildcardDomain.AnyDomainMarker)
                    continue;

                // A literal component must not contain wildcard or placeholder characters.
                if (part.IndexOfAny(new[]
                    {
                        WildcardDomain.SingleDomainMarker[0],
                        WildcardDomain.AnyDomainMarker[0],
                        PlaceholderStartMarker[0],
                        PlaceholderEndMarker[0]
                    }) >= 0)
                    return false;
            }

            if (!anyPlaceholder)
                return false;

            // Adjacent multi-matching tokens ('*' or '[Name*]') are rejected,
            // mirroring the WildcardDomain rule that forbids '*.*' (avoids ambiguous captures).
            for (var i = 0; i < parts.Length - 1; i++)
            {
                if (IsMultiMatchingToken(parts[i]) && IsMultiMatchingToken(parts[i + 1]))
                    return false;
            }

            return true;
        }

        private static bool IsMultiMatchingToken(string component)
        {
            return component == WildcardDomain.AnyDomainMarker ||
                   (TryParsePlaceholderToken(component, out _, out var isMulti, out _) && isMulti);
        }

        private static bool TryParsePlaceholderToken(string component, out string name, out bool isMulti, out bool isNegated)
        {
            var match = PlaceholderTokenRegex.Match(component);

            // A negated multi placeholder ('[!Name*]') is not a valid token: the ambiguous split of a
            // multi-component "not equal" comparison has no deterministic semantics.
            if (!match.Success || (match.Groups["negated"].Success && match.Groups["multi"].Success))
            {
                name = null;
                isMulti = false;
                isNegated = false;
                return false;
            }

            name = match.Groups["name"].Value;
            isMulti = match.Groups["multi"].Success;
            isNegated = match.Groups["negated"].Success;
            return true;
        }

        /// <summary>
        /// Backtracking matcher over domain components, modeled after <see cref="WildcardDomain"/>'s
        /// distance calculation, extended with capture recording (capture mode, <paramref name="boundValues"/> is null)
        /// or comparison against previously captured values (bound mode, <paramref name="captures"/> is null).
        /// The pattern is pre-tokenized, so the match path performs string comparisons only.
        /// </summary>
        private static bool TryMatchRecursive(
            ReadOnlySpan<string> remainingActual,
            ReadOnlySpan<Token> remainingPattern,
            Dictionary<string, string> captures,
            IReadOnlyDictionary<string, string> boundValues)
        {
            if (remainingPattern.IsEmpty)
                return remainingActual.IsEmpty;

            var token = remainingPattern[0];

            switch (token.Kind)
            {
                case TokenKind.SingleWildcard:
                    return !remainingActual.IsEmpty &&
                           TryMatchRecursive(remainingActual.Slice(1), remainingPattern.Slice(1), captures, boundValues);

                case TokenKind.AnyWildcard:
                    // Option 1: the '*' matches zero components.
                    if (TryMatchRecursive(remainingActual, remainingPattern.Slice(1), captures, boundValues))
                        return true;

                    // Option 2: the '*' consumes one more component.
                    return !remainingActual.IsEmpty &&
                           TryMatchRecursive(remainingActual.Slice(1), remainingPattern, captures, boundValues);

                case TokenKind.Placeholder:
                    return boundValues != null
                        ? TryMatchBoundPlaceholder(remainingActual, remainingPattern, token, boundValues)
                        : TryMatchCapturingPlaceholder(remainingActual, remainingPattern, token, captures);

                default:
                    return !remainingActual.IsEmpty &&
                           token.Text == remainingActual[0] &&
                           TryMatchRecursive(remainingActual.Slice(1), remainingPattern.Slice(1), captures, boundValues);
            }
        }

        private static bool TryMatchCapturingPlaceholder(
            ReadOnlySpan<string> remainingActual,
            ReadOnlySpan<Token> remainingPattern,
            Token token,
            Dictionary<string, string> captures)
        {
            // Negated placeholders have no capture semantics
            // (rejected on the 'From' side by DependencyRule anyway).
            if (token.IsNegated || remainingActual.IsEmpty)
                return false;

            var maxLength = token.IsMulti ? remainingActual.Length : 1;

            // Shortest-first: deterministic capture semantics.
            for (var length = 1; length <= maxLength; length++)
            {
                var candidate = length == 1
                    ? remainingActual[0]
                    : string.Join(DomainPartSeparator.ToString(), remainingActual.Slice(0, length).ToArray());

                captures[token.Name] = candidate;

                if (TryMatchRecursive(remainingActual.Slice(length), remainingPattern.Slice(1), captures, boundValues: null))
                    return true;

                // Backtrack.
                captures.Remove(token.Name);
            }

            return false;
        }

        private static bool TryMatchBoundPlaceholder(
            ReadOnlySpan<string> remainingActual,
            ReadOnlySpan<Token> remainingPattern,
            Token token,
            IReadOnlyDictionary<string, string> boundValues)
        {
            if (!boundValues.TryGetValue(token.Name, out var boundValue))
                return false;

            if (token.IsNegated)
            {
                // '[!Name]' consumes exactly one component that must differ from the bound value.
                return !remainingActual.IsEmpty &&
                       remainingActual[0] != boundValue &&
                       TryMatchRecursive(remainingActual.Slice(1), remainingPattern.Slice(1), captures: null, boundValues);
            }

            // '[Name]' consumes exactly the bound value, which may span several
            // components if it was captured by a multi placeholder ('[Name*]').
            var boundComponents = boundValue.Split(DomainPartSeparator);

            if (remainingActual.Length < boundComponents.Length)
                return false;

            for (var i = 0; i < boundComponents.Length; i++)
            {
                if (remainingActual[i] != boundComponents[i])
                    return false;
            }

            return TryMatchRecursive(remainingActual.Slice(boundComponents.Length), remainingPattern.Slice(1), captures: null, boundValues);
        }

        private WildcardDomain GetDegradedWildcardDomain()
        {
            return _degradedWildcardDomain ??= CreateDegradedWildcardDomain();
        }

        private WildcardDomain CreateDegradedWildcardDomain()
        {
            var degradedComponents = GetTokens()
                .Select(token => token.Kind == TokenKind.Placeholder
                    ? (token.IsMulti ? WildcardDomain.AnyDomainMarker : WildcardDomain.SingleDomainMarker)
                    : token.Text);

            var degradedString = string.Join(DomainPartSeparator.ToString(), degradedComponents);

            // The degraded form is valid by construction (contains at least one wildcard,
            // adjacent multi tokens are rejected in IsValid), so validation is skipped.
            return new WildcardDomain(degradedString, validate: false);
        }
    }
}
