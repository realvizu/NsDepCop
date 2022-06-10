#nullable enable

namespace Codartis.NsDepCop.Config;

/// <summary>
/// Specifies a location for a dependency rule.
/// LineNumber and LinePosition are 1-based.
/// </summary>
public record RuleLocation(
    string ConfigFilePath,
    ConfigFileScope ConfigFileScope,
    int LineNumber,
    int LinePosition
);