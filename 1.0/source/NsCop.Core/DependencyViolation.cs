using Roslyn.Compilers.Common;

namespace Codartis.NsCop.Core
{
    /// <summary>
    /// Represents the info that describes a namespace dependency violation.
    /// </summary>
    public class DependencyViolation
    {
        /// <summary>
        /// The syntax node where the violation was found.
        /// </summary>
        public CommonSyntaxNode SyntaxNode { get; private set; }

        /// <summary>
        /// The illegal dependency. Contains the two namespaces.
        /// </summary>
        public Dependency IllegalDependency { get; private set; }

        /// <summary>
        /// The symbol for the referencing type.
        /// </summary>
        public ISymbol ReferencingType { get; private set; }

        /// <summary>
        /// The symbol for the referenced type.
        /// </summary>
        public ISymbol ReferencedType { get; private set; }

        /// <summary>
        /// Initilaizes a new instance.
        /// </summary>
        /// <param name="syntaxNode"></param>
        /// <param name="illegalDependency"></param>
        /// <param name="referencingType"></param>
        /// <param name="referencedType"></param>
        public DependencyViolation(CommonSyntaxNode syntaxNode, Dependency illegalDependency, ISymbol referencingType, ISymbol referencedType)
        {
            SyntaxNode = syntaxNode;
            IllegalDependency = illegalDependency;
            ReferencingType = referencingType;
            ReferencedType = referencedType;
        }

        /// <summary>
        /// Returns the dependency violation info in readable format.
        /// </summary>
        /// <returns>The dependency violation info in readable format.</returns>
        public override string ToString()
        {
            return string.Format("Illegal namespace reference: {0}->{1} (Symbol '{3}' in type '{2}' is type of '{4}'.)",
                IllegalDependency.From,
                IllegalDependency.To,
                ReferencingType.ToDisplayString(),
                SyntaxNode.ToString(),
                ReferencedType.ToDisplayString());
        }
    }
}
