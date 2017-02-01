namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// The configuration used for a project.
    /// </summary>
    public interface IProjectConfig : IRuleConfig
    {
        /// <summary>
        /// A value indicating whether analysis is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// A value representing the severity of an issue.
        /// </summary>
        IssueKind IssueKind { get; }

        /// <summary>
        /// The importance level of NsDepCop information messages. 
        /// Influences whether messages are emitted or suppressed by the host.
        /// </summary>
        Importance InfoImportance { get; }

        /// <summary>
        /// The type of parser used by the analyzer.
        /// </summary>
        Parsers Parser { get; }

        IProjectConfig WithParser(Parsers parser);

        IProjectConfig WithInfoImportance(Importance infoImportance);
    }
}