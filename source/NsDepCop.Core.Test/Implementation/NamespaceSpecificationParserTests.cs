using System;
using Codartis.NsDepCop.Core.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Implementation
{
    [TestClass]
    public class NamespaceSpecificationParserTests
    {
        [TestMethod]
        public void Parse_Namespace()
        {
            NamespaceSpecificationParser.Parse(".").ShouldEqual(new Namespace("."));
            NamespaceSpecificationParser.Parse("A.B").ShouldEqual(new Namespace("A.B"));
        }

        [TestMethod]
        public void Parse_NamespaceTree()
        {
            NamespaceSpecificationParser.Parse("*").ShouldEqual(new NamespaceTree("*"));
            NamespaceSpecificationParser.Parse("A.*").ShouldEqual(new NamespaceTree("A.*"));
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_Invalid1()
        {
            NamespaceSpecificationParser.Parse("..");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_Invalid2()
        {
            NamespaceSpecificationParser.Parse(".A");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_Invalid3()
        {
            NamespaceSpecificationParser.Parse("A.");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parse_InvalidUseOfAny()
        {
            NamespaceSpecificationParser.Parse("*.*");
        }
    }
}
