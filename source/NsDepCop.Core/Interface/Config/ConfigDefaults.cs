using System;

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
        public const IssueKind MaxIssueCountSeverity = IssueKind.Warning;
        public const bool ChildCanDependOnParentImplicitly = false;
        public const Importance InfoImportance = Importance.Normal;

        public static readonly TimeSpan[] AnalyzerServiceCallRetryTimeSpans =
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(5000),
        };
    }
}