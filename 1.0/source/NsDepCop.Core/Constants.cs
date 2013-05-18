namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Constant values used in the NsDepCop tool.
    /// </summary>
    public static class Constants
    {
        public const string TOOL_NAME = "NsDepCop";
        public const bool DEFAULT_IS_ENABLED_VALUE = false;
        public const IssueKind DEFAULT_ISSUE_KIND = IssueKind.Warning;
        public const string DEFAULT_CONFIG_FILE_NAME = "config.nsdepcop";
        public const string MSBUILD_CODE_ISSUE = "NSDEPCOP01";
        public const string MSBUILD_CODE_TOO_MANY_ISSUES = "NSDEPCOP02";
        public const string MSBUILD_CODE_EXCEPTION = "NSDEPCOPEX";
        public const string MSBUILD_CODE_NO_CONFIG_FILE = "NSDEPCOP03";
        public const string MSBUILD_CODE_CONFIG_DISABLED = "NSDEPCOP04";
        public const int MAX_ISSUE_REPORTED_PER_FILE = 100;
        public const int MAX_ISSUE_REPORTED_PER_PROJECT = 100;
    }
}
