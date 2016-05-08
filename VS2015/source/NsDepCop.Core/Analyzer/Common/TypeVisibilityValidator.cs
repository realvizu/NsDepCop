using System.Collections.Immutable;

namespace Codartis.NsDepCop.Core.Analyzer.Common
{
    /// <summary>
    /// Validates dependencies to a set of allowed/disallowed rules and namespace surface definitions.
    /// </summary>
    public class TypeVisibilityValidator
    {
        private readonly ImmutableDictionary<string, ImmutableHashSet<string>> _visibleTypesPerNamespaces;

        public TypeVisibilityValidator(ImmutableDictionary<string, ImmutableHashSet<string>> visibleTypesPerNamespaces)
        {
            _visibleTypesPerNamespaces = visibleTypesPerNamespaces ?? ImmutableDictionary<string, ImmutableHashSet<string>>.Empty;
        }

        public virtual bool IsTypeVisible(string namespaceName, string typeName)
        {
            ImmutableHashSet<string> visibleTypeNames;

            if (_visibleTypesPerNamespaces.TryGetValue(namespaceName, out visibleTypeNames))
                return visibleTypeNames.Contains(typeName);

            return true;
        }

        protected static string IsVisibleToString(bool isVisible)
        {
            return isVisible ? "visible" : "not visible";
        }
    }
}
