using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests that the analyzer handles various C# 7.3 constructs correctly.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public class Cs7_3_Tests
    {
        [Fact]
        public void Cs7_3_AttributeOnPropertyBackingField()
        {
            SourceTestSpecification.Create()

                // [Serializable]
                .ExpectInvalidSegment(5, 6, 18)

                // [field: NonSerialized]
                .ExpectInvalidSegment(8, 17, 30)

                .Execute();
        }
    }
}
