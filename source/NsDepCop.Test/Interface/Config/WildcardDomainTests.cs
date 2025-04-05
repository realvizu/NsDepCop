using System;
using System.Diagnostics.CodeAnalysis;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class WildcardDomainTests
    {
        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        [InlineData("*.A.*")]
        [InlineData("*.A.?")]
        [InlineData("*.A.?.B")]
        [InlineData("A.?.?.B")]
        [InlineData("A.*.?.?.B")]
        public void Create_Works(string wildcardDomainString)
        {
            new WildcardDomain(wildcardDomainString).ToString().Should().Be(wildcardDomainString);
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardDomain(null));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A.B")]
        public void Create_NotAWildcardDomain_ThrowsFormatException(string wildcardDomainString)
        {
            Assert.Throws<FormatException>(() => new WildcardDomain(wildcardDomainString));
        }

        [Theory]
        [InlineData("")]
        [InlineData(".")]
        [InlineData("A..")]
        [InlineData("A.B.")]
        [InlineData("A.B?.C")]
        [InlineData("A*.B")]
        [InlineData("A.**.B")]
        [InlineData("A.*.*.B")]
        [InlineData(".*.B")]
        public void Create_InvalidWildcardDomain_ThrowsFormatException(string wildcardDomainString)
        {
            Assert.Throws<FormatException>(() => new WildcardDomain(wildcardDomainString));
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void Equals_Works()
        {
            (new WildcardDomain("A.*") == new WildcardDomain("A.*")).Should().BeTrue();
            (new WildcardDomain("A.*") == new WildcardDomain("B.*")).Should().BeFalse();
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void AnyDomain_IsEqualToOtherInstanceOfAnyDomain()
        {
            (new WildcardDomain("*") == new WildcardDomain("*")).Should().BeTrue();
        }
    }
}
