using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests the analyzer with the Roslyn 1.x adapter for same basic test cases.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    public class Roslyn1xSmokeTests
    {
        [Fact]
        public void AnalyzerFeature_AllowedDependency()
        {
            SourceTestSpecification.Create().Execute();
        }

        [Fact]
        public void Cs6_EveryUserDefinedTypeKind()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 17, 24)
                .ExpectInvalidSegment(8, 17, 29)
                .ExpectInvalidSegment(9, 17, 25)
                .ExpectInvalidSegment(10, 17, 23)
                .ExpectInvalidSegment(11, 17, 27)
                .Execute();
        }
    }
}