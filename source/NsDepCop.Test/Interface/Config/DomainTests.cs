using System;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class DomainTests
    {
        [Theory]
        [InlineData("A")]
        [InlineData("A.B")]
        [InlineData(".")]
        public void Create_Works(string domainString)
        {
            new Domain(domainString).ToString().Should().Be(domainString);
        }

        [Theory]
        [InlineData("<global namespace>")]
        [InlineData("")]
        public void Create_GlobalDomainRepresentationNormalized(string globalDomainString)
        {
            new Domain(globalDomainString).ToString().Should().Be(".");
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            Assert.Throws<ArgumentNullException>(() => new Domain(null));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        public void Create_AnyDomain_ThrowsFormatException(string domainString)
        {
            Assert.Throws<FormatException>(() => new Domain(domainString));
        }

        [Fact]
        public void IsSubDomainOf_Works()
        {
            new Domain("A").IsSubDomain(new Domain(".")).Should().BeTrue();
            new Domain("A.B").IsSubDomain(new Domain(".")).Should().BeTrue();

            new Domain("A.B").IsSubDomain(new Domain("A")).Should().BeTrue();
            new Domain("A.B.C").IsSubDomain(new Domain("A")).Should().BeTrue();

            new Domain(".").IsSubDomain(new Domain("A")).Should().BeFalse();
            new Domain("A").IsSubDomain(new Domain("A")).Should().BeFalse();
            new Domain("A").IsSubDomain(new Domain("A.B")).Should().BeFalse();
        }

        [Fact]
        public void Equals_Works()
        {
            (new Domain("A") == new Domain("A")).Should().BeTrue();
            (new Domain("A") == new Domain("B")).Should().BeFalse();
        }

        [Fact]
        public void GlobalDomain_IsEqualToOtherInstanceOfGlobalDomain()
        {
            (new Domain(".") == Domain.GlobalDomain).Should().BeTrue();
        }
    }
}
