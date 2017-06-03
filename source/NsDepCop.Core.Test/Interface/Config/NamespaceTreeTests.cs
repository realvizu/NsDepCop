using System;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    public class NamespaceTreeTests
    {
        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        public void Create_Works(string namespaceTreeString)
        {
            new NamespaceTree(namespaceTreeString).ToString().Should().Be(namespaceTreeString);
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            Assert.Throws<ArgumentNullException>(() => new NamespaceTree(null));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("A.B")]
        public void Create_NotANamespaceTree_TopNamespace_ThrowsFormatException(string namespaceTreeString)
        {
            Assert.Throws<FormatException>(() => new NamespaceTree(namespaceTreeString));
        }

        [Fact]
        public void Equals_Works()
        {
            (new NamespaceTree("A.*") == new NamespaceTree("A.*")).Should().BeTrue();
            (new NamespaceTree("A.*") == new NamespaceTree("B.*")).Should().BeFalse();
        }

        [Fact]
        public void AnyNamespace_IsEqualToOtherInstanceOfAnyNamespace()
        {
            (new NamespaceTree("*") == NamespaceTree.AnyNamespace).Should().BeTrue();
        }
    }
}
