using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Describes the config for a C# project. Immutable.
    /// </summary>
    internal class ProjectConfig : IProjectConfig
    {
        public bool IsEnabled { get; }
        public IssueKind IssueKind { get; }
        public Importance InfoImportance { get; }
        public Parsers Parser { get; }

        public bool ChildCanDependOnParentImplicitly { get; }
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules { get; }
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
        public int MaxIssueCount { get; }

        public ProjectConfig(
            bool isEnabled,
            IssueKind issueKind,
            Importance infoImportance,
            Parsers parser,
            bool childCanDependOnParentImplicitly,
            ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> allowRules,
            ImmutableHashSet<NamespaceDependencyRule> disallowRules,
            ImmutableDictionary<Namespace, TypeNameSet> visibleTypesByNamespace,
            int maxIssueCount)
        {
            IsEnabled = isEnabled;
            IssueKind = issueKind;
            InfoImportance = infoImportance;
            Parser = parser;

            ChildCanDependOnParentImplicitly = childCanDependOnParentImplicitly;
            AllowRules = allowRules;
            DisallowRules = disallowRules;
            VisibleTypesByNamespace = visibleTypesByNamespace;
            MaxIssueCount = maxIssueCount;
        }
    }
}