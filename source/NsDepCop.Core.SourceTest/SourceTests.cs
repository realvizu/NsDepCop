using Xunit;

namespace Codartis.NsDepCop.Core.SourceTest
{
    public class SourceTests
    {
        [Fact]
        public void AliasQualifiedName()
        {
            SourceTestSpecification.Create(nameof(AliasQualifiedName)).ExpectEnvalidSegment(7, 25, 31).Execute();
        }
    }
}
