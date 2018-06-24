using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests that the analyzer handles various C# 7.1 constructs correctly.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public class Cs7_1Tests
    {
        [Fact]
        public void Cs7_1_DefaultLiteral()
        {
            SourceTestSpecification.Create()

                // public Class2 M1(Class2 p1 = default)
                .ExpectInvalidSegment(11, 16, 22)
                .ExpectInvalidSegment(11, 26, 32)
                .ExpectInvalidSegment(11, 38, 45)

                // Class2 x = default;
                .ExpectInvalidSegment(13, 13, 19)
                .ExpectInvalidSegment(13, 24, 31)

                // var a = new[] { default, x };
                .ExpectInvalidSegment(14, 13, 16)
                .ExpectInvalidSegment(14, 28, 35)
                .ExpectInvalidSegment(14, 37, 38)

                // return default;
                .ExpectInvalidSegment(15, 20, 27)

                .Execute();
        }
    }
}
