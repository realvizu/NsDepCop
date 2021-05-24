using System;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class NamespaceTests
    {
        [Theory]
        [InlineData("A")]
        [InlineData("A.B")]
        [InlineData(".")]
        public void Create_Works(string namespaceString)
        {
            new Namespace(namespaceString).ToString().Should().Be(namespaceString);
        }

        [Theory]
        [InlineData("<global namespace>")]
        [InlineData("")]
        public void Create_GlobalNamespaceRepresentationNormalized(string globalNamespaceString)
        {
            new Namespace(globalNamespaceString).ToString().Should().Be(".");
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            Assert.Throws<ArgumentNullException>(() => new Namespace(null));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        public void Create_AnyNamespace_ThrowsFormatException(string namespaceString)
        {
            Assert.Throws<FormatException>(() => new Namespace(namespaceString));
        }

        [Fact]
        public void IsSubnamespaceOf_Works()
        {
            new Namespace("A").IsSubnamespaceOf(new Namespace(".")).Should().BeTrue();
            new Namespace("A.B").IsSubnamespaceOf(new Namespace(".")).Should().BeTrue();

            new Namespace("A.B").IsSubnamespaceOf(new Namespace("A")).Should().BeTrue();
            new Namespace("A.B.C").IsSubnamespaceOf(new Namespace("A")).Should().BeTrue();

            new Namespace(".").IsSubnamespaceOf(new Namespace("A")).Should().BeFalse();
            new Namespace("A").IsSubnamespaceOf(new Namespace("A")).Should().BeFalse();
            new Namespace("A").IsSubnamespaceOf(new Namespace("A.B")).Should().BeFalse();
        }

        [Fact]
        public void Equals_Works()
        {
            (new Namespace("A") == new Namespace("A")).Should().BeTrue();
            (new Namespace("A") == new Namespace("B")).Should().BeFalse();
        }

        [Fact]
        public void GlobalNamespace_IsEqualToOtherInstanceOfGlobalNamespace()
        {
            (new Namespace(".") == Namespace.GlobalNamespace).Should().BeTrue();
        }
    }
}
