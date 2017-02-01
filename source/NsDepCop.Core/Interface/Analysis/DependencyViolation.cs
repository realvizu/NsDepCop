namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Describes a dependency violation between two types.
    /// </summary>
    public class DependencyViolation
    {
        /// <summary>
        /// The illegal type dependency.
        /// </summary>
        public TypeDependency TypeDependency { get; }

        /// <summary>
        /// Specifies the source file and the start and end positions of the text that caused this dependency violation.
        /// </summary>
        public SourceSegment SourceSegment { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="typeDependency">The illegal type dependency.</param>
        /// <param name="sourceSegment">Info about the dependency violation's location in the source.</param>
        public DependencyViolation(TypeDependency typeDependency, SourceSegment sourceSegment)
        {
            TypeDependency = typeDependency;
            SourceSegment = sourceSegment;
        }

        /// <summary>
        /// Returns the dependency violation info in readable format.
        /// </summary>
        /// <returns>The dependency violation info in readable format.</returns>
        public override string ToString() => IssueDefinitions.IllegalDependencyIssue.GetDynamicDescription(this);
    }
}
