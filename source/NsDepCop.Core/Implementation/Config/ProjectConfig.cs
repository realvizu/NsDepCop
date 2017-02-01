using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Describes the config for a C# project. Immutable.
    /// </summary>
    internal class ProjectConfig : IProjectConfig
    {
        public bool IsEnabled { get; set; }
        public IssueKind IssueKind { get; set; }
        public Importance InfoImportance { get; set; }
        public Parsers Parser { get; set; }

        public bool ChildCanDependOnParentImplicitly { get; set; }
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; set; }
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules { get; set; }
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; set; }
        public int MaxIssueCount { get; set; }

        public ProjectConfig()
        {
            IsEnabled = ConfigDefaults.IsEnabled;
            IssueKind = ConfigDefaults.IssueKind;
            InfoImportance = ConfigDefaults.InfoImportance;
            Parser = ConfigDefaults.Parser;

            ChildCanDependOnParentImplicitly = ConfigDefaults.ChildCanDependOnParentImplicitly;
            AllowRules = ImmutableDictionary<NamespaceDependencyRule, TypeNameSet>.Empty;
            DisallowRules = ImmutableHashSet<NamespaceDependencyRule>.Empty;
            VisibleTypesByNamespace = ImmutableDictionary<Namespace, TypeNameSet>.Empty;
            MaxIssueCount = ConfigDefaults.MaxIssueReported;
        }

        public ProjectConfig(IProjectConfig projectConfig)
        {
            IsEnabled = projectConfig.IsEnabled;
            IssueKind = projectConfig.IssueKind;
            InfoImportance = projectConfig.InfoImportance;
            Parser = projectConfig.Parser;

            ChildCanDependOnParentImplicitly = projectConfig.ChildCanDependOnParentImplicitly;
            AllowRules = projectConfig.AllowRules;
            DisallowRules = projectConfig.DisallowRules;
            VisibleTypesByNamespace = projectConfig.VisibleTypesByNamespace;
            MaxIssueCount = projectConfig.MaxIssueCount;
        }

        public IProjectConfig WithParser(Parsers parser) => new ProjectConfig(this) { Parser = parser };
        public IProjectConfig WithInfoImportance(Importance infoImportance) => new ProjectConfig(this) { InfoImportance = infoImportance };
    }
}