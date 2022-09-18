using System;
using System.Diagnostics.CodeAnalysis;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class WildcardNamespaceTests
    {
        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        [InlineData("*.A.*")]
        [InlineData("*.A.?")]
        [InlineData("*.A.?.B")]
        [InlineData("A.?.?.B")]
        [InlineData("A.*.?.?.B")]
        public void Create_Works(string wildcardNamespaceString)
        {
            new WildcardNamespace(wildcardNamespaceString).ToString().Should().Be(wildcardNamespaceString);
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            Assert.Throws<ArgumentNullException>(() => new WildcardNamespace(null));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A.B")]
        public void Create_NotAWildcardNamespace_ThrowsFormatException(string wildcardNamespaceString)
        {
            Assert.Throws<FormatException>(() => new WildcardNamespace(wildcardNamespaceString));
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
        public void Create_InvalidWildcardNamespace_ThrowsFormatException(string wildcardNamespaceString)
        {
            Assert.Throws<FormatException>(() => new WildcardNamespace(wildcardNamespaceString));
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void Equals_Works()
        {
            (new WildcardNamespace("A.*") == new WildcardNamespace("A.*")).Should().BeTrue();
            (new WildcardNamespace("A.*") == new WildcardNamespace("B.*")).Should().BeFalse();
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void AnyNamespace_IsEqualToOtherInstanceOfAnyNamespace()
        {
            (new WildcardNamespace("*") == new WildcardNamespace("*")).Should().BeTrue();
        }
    }
}
