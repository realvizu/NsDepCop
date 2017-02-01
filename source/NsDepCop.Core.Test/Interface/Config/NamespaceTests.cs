using System;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    [TestClass]
    public class NamespaceTests
    {
        [TestMethod]
        public void Create_Works()
        {
            new Namespace("A").ToString().ShouldEqual("A");
            new Namespace("A.B").ToString().ShouldEqual("A.B");
            new Namespace(".").ToString().ShouldEqual(".");
        }

        [TestMethod]
        public void Create_GlobalNamespaceRepresentationNormalized()
        {
            new Namespace("<global namespace>").ToString().ShouldEqual(".");
            new Namespace("").ToString().ShouldEqual(".");
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
            new Namespace("A").IsSubnamespaceOf(new Namespace(".")).ShouldBeTrue();
            new Namespace("A.B").IsSubnamespaceOf(new Namespace(".")).ShouldBeTrue();

            new Namespace("A.B").IsSubnamespaceOf(new Namespace("A")).ShouldBeTrue();
            new Namespace("A.B.C").IsSubnamespaceOf(new Namespace("A")).ShouldBeTrue();

            new Namespace(".").IsSubnamespaceOf(new Namespace("A")).ShouldBeFalse();
            new Namespace("A").IsSubnamespaceOf(new Namespace("A")).ShouldBeFalse();
            new Namespace("A").IsSubnamespaceOf(new Namespace("A.B")).ShouldBeFalse();
        }

        [TestMethod]
        public void Equals_Works()
        {
            (new Namespace("A") == new Namespace("A")).ShouldBeTrue();
            (new Namespace("A") == new Namespace("B")).ShouldBeFalse();

            (new Namespace(".") == Namespace.GlobalNamespace).ShouldBeTrue();
        }

        [TestMethod]
        public void GlobalNamespace_IsEqualToOtherInstanceOfGlobalNamespace()
        {
            (new Namespace(".") == Namespace.GlobalNamespace).ShouldBeTrue();
        }
    }
}
