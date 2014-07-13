namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Constant values used in the NsDepCop tool.
    /// </summary>
    public static class Constants
    {
        public const string TOOL_NAME = "NsDepCop";
        public const string DEFAULT_CONFIG_FILE_NAME = "config.nsdepcop";

        public const string DIAGNOSTIC_ID_ILLEGAL_NS_DEP = "NSDEPCOP01";
        public const string DIAGNOSTIC_DESC_ILLEGAL_NS_DEP = "Illegal namespace reference.";
        public const string DIAGNOSTIC_FORMAT_ILLEGAL_NS_DEP = "Illegal namespace reference: {0}->{1} (Symbol '{3}' in type '{2}' is type of '{4}'.";

        public const string DIAGNOSTIC_ID_TOO_MANY_ISSUES = "NSDEPCOP02";
        public const string DIAGNOSTIC_DESC_TOO_MANY_ISSUES = "Too many issues, analysis was stopped.";

        public const string DIAGNOSTIC_ID_NO_CONFIG_FILE = "NSDEPCOP03";
        public const string DIAGNOSTIC_DESC_NO_CONFIG_FILE = "No config file found, analysis skipped.";

        public const string DIAGNOSTIC_ID_CONFIG_DISABLED = "NSDEPCOP04";
        public const string DIAGNOSTIC_DESC_CONFIG_DISABLED = "Analysis is disabled in the nsdepcop config file.";

        public const string DIAGNOSTIC_ID_CONFIG_EXCEPTION = "NSDEPCOP05";
        public const string DIAGNOSTIC_DESC_CONFIG_EXCEPTION = "Error loading NsDepCop config.";
        public const string DIAGNOSTIC_FORMAT_CONFIG_EXCEPTION = "Error loading NsDepCop config: {0}";
    }
}
