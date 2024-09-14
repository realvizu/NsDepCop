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
                .ExpectInvalidSegment(7, 24, 28)
                .Execute();
        }

        [Fact]
        public void AnalyzerFeature_ExcludedFiles()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_ExcludedFiles_WithWildcard()
        {
            SourceTestSpecification.Create().Execute();
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
        public void AnalyzerFeature_ParentCanDependOnChildImplicitly()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void AnalyzerFeature_VisibleMembersOfNamespace()
        {
            string[] members = new[] { "VisibleType", "OnlyGenericIsVisibleType`1" };

            SourceTestSpecification.Create()
                
                // A -> C
                .ExpectInvalidSegment(6, 19, 32, members)
                .ExpectInvalidSegment(8, 19, 43, members)
                .ExpectInvalidSegment(9, 19, 47, members)

                // B -> C
                .ExpectInvalidSegment(20, 19, 32, members)
                .ExpectInvalidSegment(22, 19, 43, members)
                .ExpectInvalidSegment(23, 19, 47, members)
                
                .Execute();
        }

        [Fact]
        public void AnalyzerFeature_VisibleMembersOfAllowedRule()
        {
            string[] members = new[] { "VisibleType", "OnlyGenericIsVisibleType`1" };
            
            SourceTestSpecification.Create()

                // A -> C
                .ExpectInvalidSegment(6, 19, 32, members)
                .ExpectInvalidSegment(8, 19, 43, members)
                .ExpectInvalidSegment(9, 19, 47, members)

                .Execute();
        }

        [Fact]
        public void AnalyzerFeature_WithTopLevelStatement()
        {
            SourceTestSpecification.Create()

                .ExpectInvalidSegment(1, 8, 12)

                .Execute(Microsoft.CodeAnalysis.OutputKind.ConsoleApplication);
        }
    }
}