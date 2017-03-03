using System;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    [TestClass]
    public class NamespaceTreeTests
    {
        [TestMethod]
        public void Create_Works()
        {
            new NamespaceTree("*").ToString().Should().Be("*");
            new NamespaceTree("A.*").ToString().Should().Be("A.*");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_WithNull_ThrowsArgumentNullExceptionn()
        {
            new NamespaceTree(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Create_NotANamespaceTree_TopNamespace_ThrowsFormatException()
        {
            new NamespaceTree("A");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Create_NotANamespaceTree_Subnamespace_ThrowsFormatException()
        {
            new NamespaceTree("A.B");
        }

        [TestMethod]
        public void Equals_Works()
        {
            (new NamespaceTree("A.*") == new NamespaceTree("A.*")).Should().BeTrue();
            (new NamespaceTree("A.*") == new NamespaceTree("B.*")).Should().BeFalse();
        }

        [TestMethod]
        public void AnyNamespace_IsEqualToOtherInstanceOfAnyNamespace()
        {
            (new NamespaceTree("*") == NamespaceTree.AnyNamespace).Should().BeTrue();
        }
    }
}
