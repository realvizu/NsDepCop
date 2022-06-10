#nullable enable

namespace Codartis.NsDepCop.Config;

/// <summary>
/// Indicates the scope affected by a config file.
/// </summary>
public enum ConfigFileScope
{
    /// <summary>
    /// The config file affects a single compilation.
    /// </summary>
    SingleCompilation,

    /// <summary>
    /// The config file affects multiple compilations.
    /// </summary>
    MultipleCompilations
}