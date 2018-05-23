namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// Enumerates the issue types that can be emitted by an analyzer.
    /// </summary>
    public enum IssueType
    {
        IllegalDependency,
        TooManyIssues,
        ConfigException,
    }
}
