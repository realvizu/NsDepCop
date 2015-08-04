namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Constant values used in the NsDepCop tool.
    /// </summary>
    public static class Constants
    {
        public const string TOOL_NAME = "NsDepCop";
        public const string DEFAULT_CONFIG_FILE_NAME = "config.nsdepcop";

        public static IssueDescriptor IllegalDependencyIssue = new IssueDescriptor(
            "NSDEPCOP01",
            IssueKind.Warning,
            "Illegal namespace reference.",
            "Illegal namespace reference: {0}->{1} (Symbol '{3}' in type '{2}' references type '{4}'.)");

        public static IssueDescriptor TooManyIssuesIssue = new IssueDescriptor(
            "NSDEPCOP02",
            IssueKind.Warning,
            "Too many issues, analysis was stopped.");

        public static IssueDescriptor NoConfigFileIssue = new IssueDescriptor(
            "NSDEPCOP03",
            IssueKind.Info,
            "No config file found, analysis skipped.");

        public static IssueDescriptor ConfigDisabledIssue = new IssueDescriptor(
            "NSDEPCOP04",
            IssueKind.Info,
            "Analysis is disabled in the nsdepcop config file.");

        public static IssueDescriptor ConfigExceptionIssue = new IssueDescriptor(
            "NSDEPCOP05",
            IssueKind.Error,
            "Error loading NsDepCop config.",
            "Error loading NsDepCop config: {0}");
    }
}
