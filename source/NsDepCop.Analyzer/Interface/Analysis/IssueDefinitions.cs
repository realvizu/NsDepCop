using Codartis.NsDepCop.Interface.Config;

namespace Codartis.NsDepCop.Interface.Analysis
{
    /// <summary>
    /// Defines the types of issues that the tool can report.
    /// </summary>
    public static class IssueDefinitions
    {
        public static readonly IssueDescriptor IllegalDependencyIssue =
            new IssueDescriptor(
                "NSDEPCOP01",
                IssueKind.Warning,
                "Illegal namespace reference.");

        public static readonly IssueDescriptor TooManyIssuesIssue = 
            new IssueDescriptor(
                "NSDEPCOP02",
                IssueKind.Warning,
                "Too many issues, analysis was stopped.");

        public static readonly IssueDescriptor NoConfigFileIssue = 
            new IssueDescriptor(
                "NSDEPCOP03",
                IssueKind.Info,
                "No config file found, analysis skipped.");

        public static readonly IssueDescriptor ConfigDisabledIssue = 
            new IssueDescriptor(
                "NSDEPCOP04",
                IssueKind.Info,
                "Analysis is disabled in the nsdepcop config file.");

        public static readonly IssueDescriptor ConfigExceptionIssue =
            new IssueDescriptor(
                "NSDEPCOP05",
                IssueKind.Error,
                "Error loading NsDepCop config.");

        public static readonly IssueDescriptor ToolDisabledIssue =
            new IssueDescriptor(
                "NSDEPCOP06",
                IssueKind.Info,
                $"Analysis is disabled with environment variable {ProductConstants.DisableToolEnvironmentVariableName}.");

    }
}