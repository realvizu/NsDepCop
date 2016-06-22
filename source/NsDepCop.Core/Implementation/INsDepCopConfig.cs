using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Represents the configuration of the tool.
    /// </summary>
    public interface INsDepCopConfig
    {
        /// <summary>
        /// A value indicating whether analysis is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// A value representing the severity of an issue.
        /// </summary>
        IssueKind IssueKind { get; }

        /// <summary>
        /// The max number of issues reported.
        /// </summary>
        int MaxIssueCount { get; }

        /// <summary>
        /// True means that all child namespaces can depend on any of their parent namespaces without an explicit Allowed rule.
        /// True is in line with how C# type resolution works: it searches parent namespaces without an explicit using statement.
        /// False means that all dependencies must be explicitly allowed with a rule.
        /// False is the default for backward compatibility.
        /// </summary>
        bool ChildCanDependOnParentImplicitly { get; }

        /// <summary>
        /// The set of allowed dependencies.
        /// </summary>
        ImmutableHashSet<Dependency> AllowedDependencies { get; }

        /// <summary>
        /// The set of disallowed dependencies.
        /// </summary>
        ImmutableHashSet<Dependency> DisallowedDependencies { get; }

        /// <summary>
        /// Dictionary of visible type by namespace. The Key is the name of a namespace, 
        /// the Value is a set of type names defines in the namespace and visible outside of the namespace.
        /// </summary>
        ImmutableDictionary<string, ImmutableHashSet<string>> VisibleTypesByNamespace { get; }
    }
}
