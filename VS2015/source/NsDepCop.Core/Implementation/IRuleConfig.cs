using System.Collections.Immutable;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Represents the dependency rule configuration.
    /// </summary>
    public interface IRuleConfig
    {
        /// <summary>
        /// True means that all child namespaces can depend on any of their parent namespaces without an explicit Allowed rule.
        /// True is in line with how C# type resolution works: it searches parent namespaces without an explicit using statement.
        /// False means that all dependencies must be explicitly allowed with a rule.
        /// False is the default for backward compatibility.
        /// </summary>
        bool ChildCanDependOnParentImplicitly { get; }

        /// <summary>
        /// Dictionary of allowed dependency rules. The key is a namespace dependency rule, 
        /// the value is a set of type names defined in the target namespace and visible for the source namespace(s).
        /// </summary>
        ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules { get; }

        /// <summary>
        /// The set of disallowed dependency rules.
        /// </summary>
        ImmutableHashSet<NamespaceDependencyRule> DisallowRules { get; }

        /// <summary>
        /// Dictionary of visible types by target namespace. The Key is the name of a namespace, 
        /// the Value is a set of type names defined in the namespace and visible outside of the namespace.
        /// </summary>
        ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace { get; }
    }
}
