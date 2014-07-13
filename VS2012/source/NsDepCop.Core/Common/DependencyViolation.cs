namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Represents the info that describes a namespace dependency violation.
    /// </summary>
    public class DependencyViolation
    {
        /// <summary>
        /// The illegal dependency. Contains the two namespaces.
        /// </summary>
        public Dependency IllegalDependency { get; private set; }

        /// <summary>
        /// The name of the referencing type.
        /// </summary>
        public string ReferencingTypeName { get; private set; }

        /// <summary>
        /// The name of the referenced type.
        /// </summary>
        public string ReferencedTypeName { get; private set; }

        /// <summary>
        /// Specifies the source file and the start and end positions of the text that caused this dependency violation.
        /// </summary>
        public SourceSegment SourceSegment { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public DependencyViolation(Dependency illegalDependency, string referencingTypeName, string referencedTypeName, SourceSegment sourceSegment)
        {
            IllegalDependency = illegalDependency;
            ReferencingTypeName = referencingTypeName;
            ReferencedTypeName = referencedTypeName;
            SourceSegment = sourceSegment;
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
                ReferencingTypeName,
                SourceSegment.Text,
                ReferencedTypeName);
        }
    }
}
