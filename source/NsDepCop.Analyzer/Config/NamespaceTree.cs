using System;

namespace Codartis.NsDepCop.Config
{
    /// <summary>
    /// Represents a namespace tree, eg. 'System.IO.*'. Immutable.
    /// </summary>
    /// <remarks>
    /// The 'any namespace' (represented by a star '*') is also a namespace tree that contains every namespace.
    /// </remarks>
    [Serializable]
    public sealed class NamespaceTree : NamespaceSpecification
    {
        private readonly Namespace _treeRootNamespace;
        private readonly int _treeRootNamespaceLength;

        public const string AnyNamespaceMarker = "*";

        /// <summary>
        /// Represents any namespace.
        /// </summary>
        public static readonly NamespaceTree AnyNamespace = new NamespaceTree(AnyNamespaceMarker);

        /// <summary>
        /// Creates a new instance from a string representation.
        /// </summary>
        /// <param name="namespaceTreeAsString">The string representation of a namespace tree.</param>
        /// <param name="validate">True means validate the input string.</param>
        public NamespaceTree(string namespaceTreeAsString, bool validate = true)
            : base(namespaceTreeAsString, validate, IsValid)
        {
            _treeRootNamespace = GetTreeRootNamespace(namespaceTreeAsString);
            _treeRootNamespaceLength = _treeRootNamespace.ToString().Length;
        }

        public override int GetMatchRelevance(Namespace ns)
        {
            if (!this.Contains(ns))
                return 0;

            return _treeRootNamespace == Namespace.GlobalNamespace
                ? _treeRootNamespaceLength
                : _treeRootNamespaceLength + 1;
        }

        public static bool IsValid(string namespaceAsString)
        {
            if (namespaceAsString == AnyNamespaceMarker)
                return true;

            var lastSeparatorIndex = namespaceAsString.LastIndexOf('.');
            if (lastSeparatorIndex < 0)
                return false;

            var namespacePart = namespaceAsString.Substring(0, lastSeparatorIndex);
            var treeMarkerPart = namespaceAsString.Substring(lastSeparatorIndex + 1);

            return Namespace.IsValid(namespacePart) && treeMarkerPart == AnyNamespaceMarker;
        }

        public bool Contains(Namespace ns)
        {
            return this == AnyNamespace 
                || ns == _treeRootNamespace 
                || ns.IsSubnamespaceOf(_treeRootNamespace);
        }

        private static Namespace GetTreeRootNamespace(string namespaceTreeAsString)
        {
            if (namespaceTreeAsString == AnyNamespaceMarker)
                return Namespace.GlobalNamespace;

            var treeRootName = namespaceTreeAsString.Substring(0, namespaceTreeAsString.Length - 2);
            return new Namespace(treeRootName);
        }
    }
}
