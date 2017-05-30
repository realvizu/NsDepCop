using Xunit;

namespace Codartis.NsDepCop.Core.SourceTest
{
    public class SourceTests
    {
        [Fact]
        public void AliasQualifiedName()
        {
            SourceTestSpecification.Create()
                .ExpectEnvalidSegment(7, 25, 31)
                .Execute();
        }

        [Fact]
        public void ArrayType()
        {
            SourceTestSpecification.Create()
                .ExpectEnvalidSegment(7, 19, 25)
                .ExpectEnvalidSegment(11, 20, 27)
                .ExpectEnvalidSegment(12, 20, 27)
                .Execute();
        }
    }
}
