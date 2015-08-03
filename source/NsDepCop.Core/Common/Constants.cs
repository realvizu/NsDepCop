namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Constant values used in the NsDepCop tool.
    /// </summary>
    public static class Constants
    {
        public const string TOOL_NAME = "NsDepCop";
        public const string DEFAULT_CONFIG_FILE_NAME = "config.nsdepcop";

        public const string DIAGNOSTIC_ILLEGALDEP_ID = "NSDEPCOP01";
        public const string DIAGNOSTIC_ILLEGALDEP_DESC = "Illegal namespace reference.";
        public const string DIAGNOSTIC_ILLEGALDEP_FORMAT = "Illegal namespace reference: {0}->{1} (Symbol '{3}' in type '{2}' references type '{4}'.)";
        public const IssueKind DIAGNOSTIC_ILLEGALDEP_DEFAULTSEVERITY = IssueKind.Warning;

        public const string DIAGNOSTIC_TOOMANYISSUES_ID = "NSDEPCOP02";
        public const string DIAGNOSTIC_TOOMANYISSUES_DESC = "Too many issues, analysis was stopped.";
        public const IssueKind DIAGNOSTIC_TOOMANYISSUES_DEFAULTSEVERITY = IssueKind.Warning;

        public const string DIAGNOSTIC_NOCONFIGFILE_ID = "NSDEPCOP03";
        public const string DIAGNOSTIC_NOCONFIGFILE_DESC = "No config file found, analysis skipped.";
        public const IssueKind DIAGNOSTIC_NOCONFIGFILE_DEFAULTSEVERITY = IssueKind.Info;

        public const string DIAGNOSTIC_CONFIGDISABLED_ID = "NSDEPCOP04";
        public const string DIAGNOSTIC_CONFIGDISABLED_DESC = "Analysis is disabled in the nsdepcop config file.";
        public const IssueKind DIAGNOSTIC_CONFIGDISABLED_DEFAULTSEVERITY = IssueKind.Info;

        public const string DIAGNOSTIC_CONFIGEXCEPTION_ID = "NSDEPCOP05";
        public const string DIAGNOSTIC_CONFIGEXCEPTION_DESC = "Error loading NsDepCop config.";
        public const string DIAGNOSTIC_CONFIGEXCEPTION_FORMAT = "Error loading NsDepCop config: {0}";
        public const IssueKind DIAGNOSTIC_CONFIGEXCEPTION_DEFAULTSEVERITY = IssueKind.Error;
    }
}
