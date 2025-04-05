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

    private static readonly TimeSpan DefaultRegexTimeout = TimeSpan.FromMilliseconds(100);

    private readonly Regex _regex;
    private readonly RegexOptions _regexOptions;
    private readonly TimeSpan _regexTimeout;

    public RegexDomain(
        string value,
        bool validate = true,
        RegexUsageMode regexUsageMode = RegexUsageMode.Instance,
        RegexCompilationMode regexCompilationMode = RegexCompilationMode.Interpreted,
        TimeSpan? regexTimeout = null)
        : base(value, validate, IsValid)
    {
        _regexTimeout = regexTimeout ?? DefaultRegexTimeout;
        _regexOptions = RegexOptions.Singleline;

        if (regexCompilationMode == RegexCompilationMode.Compiled)
            _regexOptions |= RegexOptions.Compiled;

        if (regexUsageMode == RegexUsageMode.Instance)
            _regex = new Regex(Normalize(Value), _regexOptions, _regexTimeout);
    }

    public override int GetMatchRelevance(Domain domain)
    {
        try
        {
            if (_regex != null)
                return _regex.IsMatch(domain.ToString()) ? 1 : 0;

            return Regex.IsMatch(domain.ToString(), Normalize(Value), _regexOptions, _regexTimeout) ? 1 : 0;
        }
        catch (RegexMatchTimeoutException)
        {
            return 0;
        }
    }

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