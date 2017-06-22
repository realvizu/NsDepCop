using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests analyzer features on source files.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    public class AnalyzerFeatureTests
    {
        [Fact]
        public void AnalyzerFeature_AllowedDependency()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_DisallowedDependency()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(5, 19, 25)
                .ExpectInvalidSegment(6, 17, 29)
                .ExpectInvalidSegment(7, 27, 35)
                .Execute();
        }

        [Fact]
        public void AnalyzerFeature_SameNamespaceAlwaysAllowed()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_SameNamespaceAllowedEvenWhenVisibleMembersDefined()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_ChildCanDependOnParentImplicitly()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_VisibleMembersOfNamespace()
        {
            SourceTestSpecification.Create()
                
                // A -> C
                .ExpectInvalidSegment(6, 19, 32)
                .ExpectInvalidSegment(8, 19, 43)
                .ExpectInvalidSegment(9, 19, 47)

                // B -> C
                .ExpectInvalidSegment(20, 19, 32)
                .ExpectInvalidSegment(22, 19, 43)
                .ExpectInvalidSegment(23, 19, 47)
                
                .Execute();
        }

        [Fact]
        public void AnalyzerFeature_VisibleMembersOfAllowedRule()
        {
            SourceTestSpecification.Create()

                // A -> C
                .ExpectInvalidSegment(6, 19, 32)
                .ExpectInvalidSegment(8, 19, 43)
                .ExpectInvalidSegment(9, 19, 47)

                .Execute();
        }
    }
}