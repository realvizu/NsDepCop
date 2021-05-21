using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests that the analyzer handles various C# 7.2 constructs correctly.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public class Cs7_2_Tests
    {
        [Fact]
        public void Cs7_2_NonTrailingNamedArguments()
        {
            SourceTestSpecification.Create()

                // public void M1(Class2 p1, Class2 p2)
                .ExpectInvalidSegment(11, 24, 30)
                .ExpectInvalidSegment(11, 35, 41)

                // M1(p1: p1, p2);
                .ExpectInvalidSegment(13, 20, 22)
                .ExpectInvalidSegment(13, 24, 26)

                .Execute();
        }
    }
}
