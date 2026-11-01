// C# 14 extension members only exist in Roslyn 5.0+; the source fixture won't even compile on
// earlier compilers, so this test is built only for the matching analyzer variant.
#if ROSLYN5_0_OR_GREATER
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
                // new MyClass().MyExtensionProperty
                .ExpectInvalidSegment(11, 31, 50)
                // MyClass.MyStaticExtensionProperty
                .ExpectInvalidSegment(12, 25, 50)
                // MyClass.MyStaticExtensionMethod()
                .ExpectInvalidSegment(13, 21, 44)
                .Execute();
        }
    }
}
#endif
