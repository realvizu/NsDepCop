using System;
using System.Text.RegularExpressions;

namespace Codartis.NsDepCop.Config;

/// <summary>
/// Represents a domain specification with a regular expression pattern.
/// The given pattern must follow the dotnet regex specification found at:
/// https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference
/// </summary>
/// <remarks>
/// This class provides functionality to define a domain using a regular expression
/// and supports validation and matching of other domains against the specified pattern.
/// </remarks>
[Serializable]
public sealed class RegexDomain : DomainSpecification
{
    public const string Delimiter = "/";

    public RegexDomain(string value, bool validate = true)
        : base(value, validate, IsValid)
    {
    }

    public override int GetMatchRelevance(Domain domain)
        => Regex.IsMatch(
            domain.ToString(),
            Normalize(this.Value),
            RegexOptions.Compiled | RegexOptions.Singleline,
            TimeSpan.FromMilliseconds(100))
            ? 1
            : 0;

    private static bool IsValid(string domainAsString)
    {
        if (!domainAsString.StartsWith(Delimiter, StringComparison.Ordinal)
            || !domainAsString.EndsWith(Delimiter, StringComparison.Ordinal))
        {
            return false;
        }

        var normalizedDomainAsString = Normalize(domainAsString);
        if (string.IsNullOrWhiteSpace(normalizedDomainAsString))
            return false;

        try
        {
            _ = new Regex(normalizedDomainAsString);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string Normalize(string domainAsString)
        => domainAsString.Trim(Delimiter.ToCharArray());
}