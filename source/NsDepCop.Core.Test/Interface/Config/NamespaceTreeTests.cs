using System;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    [TestClass]
    public class NamespaceTreeTests
    {
        [TestMethod]
        public void Create_Works()
        {
            new NamespaceTree("*").ToString().ShouldEqual("*");
            new NamespaceTree("A.*").ToString().ShouldEqual("A.*");
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
            (new NamespaceTree("A.*") == new NamespaceTree("A.*")).ShouldBeTrue();
            (new NamespaceTree("A.*") == new NamespaceTree("B.*")).ShouldBeFalse();
        }

        [TestMethod]
        public void AnyNamespace_IsEqualToOtherInstanceOfAnyNamespace()
        {
            (new NamespaceTree("*") == NamespaceTree.AnyNamespace).ShouldBeTrue();
        }
    }
}
