using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests that the analyzer handles C# 14 constructs correctly.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    public class Cs14Tests
    {
        [Fact]
        public void Cs14_ExtensionMembers()
        {
            SourceTestSpecification.Create()
                // new MyClass().MyExtensionMethod() — A depends on B's extension block host type
                .ExpectInvalidSegment(9, 27, 44)
                // new MyClass().MyGenericExtensionMethod()
                .ExpectInvalidSegment(10, 27, 51)
                .Execute();
        }
    }
}
