namespace Codartis.NsDepCop.Core.Interface
{
    /// <summary>
    /// Enumerates the possible states of a dependency analyzer.
    /// </summary>
    public enum  DependencyAnalyzerState
    {
        Enabled,
        Disabled,
        NoConfigFile,
        ConfigError
    }
}
