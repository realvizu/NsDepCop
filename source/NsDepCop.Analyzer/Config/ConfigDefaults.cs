namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Defines the default values of the config properties.
    /// </summary>
    public static class ConfigDefaults
    {
        public const int InheritanceDepth = 0;
        public const bool IsEnabled = true;
        public const int MaxIssueCount = 100;
        public const bool AutoLowerMaxIssueCount = false;
        public const bool ChildCanDependOnParentImplicitly = false;
    }
}