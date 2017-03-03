using System;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Interface.Config
{
    [TestClass]
    public class NamespaceSpecificationParserTests
    {
        [TestMethod]
        public void Parse_Namespace()
        {
            NamespaceSpecificationParser.Parse(".").Should().Be(new Namespace("."));
            NamespaceSpecificationParser.Parse("A.B").Should().Be(new Namespace("A.B"));
        }

        [TestMethod]
        public void Parse_NamespaceTree()
        {
            NamespaceSpecificationParser.Parse("*").Should().Be(new NamespaceTree("*"));
            NamespaceSpecificationParser.Parse("A.*").Should().Be(new NamespaceTree("A.*"));
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
