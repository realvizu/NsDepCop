using System;

namespace Codartis.NsDepCop.Interface.Config
{
    /// <summary>
    /// Defines the default values of the config properties.
    /// </summary>
    public static class ConfigDefaults
    {
        public const int InheritanceDepth = 0;
        public const bool IsEnabled = true;
        public const IssueKind DependencyIssueSeverity = IssueKind.Warning;
        public const int MaxIssueCount = 100;
        public const IssueKind MaxIssueCountSeverity = IssueKind.Warning;
        public const bool AutoLowerMaxIssueCount = false;
        public const bool ChildCanDependOnParentImplicitly = false;
        public const Importance InfoImportance = Importance.Normal;

        public static readonly TimeSpan[] AnalyzerServiceCallRetryTimeSpans =
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(300),
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(3000),
            TimeSpan.FromMilliseconds(10000),
        };
    }
}