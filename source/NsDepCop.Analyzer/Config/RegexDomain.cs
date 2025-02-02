using System;
using System.Text.RegularExpressions;

namespace Codartis.NsDepCop.Config;

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