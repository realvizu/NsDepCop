using System;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    [TestClass]
    public class NamespaceTests
    {
        [TestMethod]
        public void Create_Works()
        {
            new Namespace("A").ToString().Should().Be("A");
            new Namespace("A.B").ToString().Should().Be("A.B");
            new Namespace(".").ToString().Should().Be(".");
        }

        [TestMethod]
        public void Create_GlobalNamespaceRepresentationNormalized()
        {
            new Namespace("<global namespace>").ToString().Should().Be(".");
            new Namespace("").ToString().Should().Be(".");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            new Namespace(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Create_AnyNamespace_ThrowsFormatException()
        {
            new Namespace("*");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Create_NamespaceTree_ThrowsFormatException()
        {
            new Namespace("A.*");
        }

        [TestMethod]
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

        [TestMethod]
        public void Equals_Works()
        {
            (new Namespace("A") == new Namespace("A")).Should().BeTrue();
            (new Namespace("A") == new Namespace("B")).Should().BeFalse();

            (new Namespace(".") == Namespace.GlobalNamespace).Should().BeTrue();
        }

        [TestMethod]
        public void GlobalNamespace_IsEqualToOtherInstanceOfGlobalNamespace()
        {
            (new Namespace(".") == Namespace.GlobalNamespace).Should().BeTrue();
        }
    }
}
