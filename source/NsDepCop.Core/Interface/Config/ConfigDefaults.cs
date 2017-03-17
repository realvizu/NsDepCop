namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Defines the default values of the config properties.
    /// </summary>
    internal static class ConfigDefaults
    {
        public const int InheritanceDepth = 0;
        public const bool IsEnabled = true;
        public const IssueKind IssueKind = Config.IssueKind.Warning;
        public const int MaxIssueCount = 100;
        public const bool ChildCanDependOnParentImplicitly = false;
        public const Importance InfoImportance = Importance.Normal;
        public const Parsers Parser = Parsers.Roslyn;
    }
}