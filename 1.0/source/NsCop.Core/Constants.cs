using Roslyn.Services;

namespace Codartis.NsCop.Core
{
    /// <summary>
    /// Constant values used in the NsCop tool.
    /// </summary>
    public static class Constants
    {
        public const string TOOL_NAME = "NsCop";
        public const bool DEFAULT_IS_ENABLED_VALUE = false;
        public const CodeIssueKind DEFAULT_CODE_ISSUE_KIND = CodeIssueKind.Warning;
        public const string DEFAULT_CONFIG_FILE_NAME = "config.nscop";
        public const string CODE_NSCOP_ISSUE = "NSCOP01";
        public const string CODE_TOO_MANY_ISSUES = "NSCOP02";
        public const int MAX_ISSUE_REPORTED = 100;
    }
}
