using System;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class XmlConfigParserTests : FileBasedTestsBase
    {
        [TestMethod]
        public void Parse_RootAttributes()
        {
            var xDocument = LoadXml("RootAttributes.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);
            config.IsEnabled.Should().BeTrue();
            config.IssueKind.ShouldBeEquivalentTo(IssueKind.Error);
            config.MaxIssueCount.ShouldBeEquivalentTo(42);
            config.ChildCanDependOnParentImplicitly.Should().BeTrue();
            config.InfoImportance.ShouldBeEquivalentTo(Importance.Normal);
            config.Parser.ShouldBeEquivalentTo(Parsers.Roslyn);
        }

        [TestMethod]
        public void Parse_AllowedRules()
        {
            var xDocument = LoadXml("AllowedRules.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var allowedRules = config.AllowRules;
            allowedRules.Should().HaveCount(3);
            {
                var types = allowedRules[new NamespaceDependencyRule("N1", "N2")];
                types.Should().BeNull();
            }
            {
                var types = allowedRules[new NamespaceDependencyRule("N3", "N4")];
                types.Should().HaveCount(2);
                types.Should().Contain("T1");
                types.Should().Contain("T2");
            }
            {
                var types = allowedRules[new NamespaceDependencyRule("N5", "N6")];
                types.Should().BeNull();
            }
        }

        [TestMethod]
        public void Parse_DisallowedRules()
        {
            var xDocument = LoadXml("DisallowedRules.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var disallowedRules = config.DisallowRules;
            disallowedRules.Should().HaveCount(2);

            disallowedRules.Should().Contain(new NamespaceDependencyRule("N1", "N2"));
            disallowedRules.Should().Contain(new NamespaceDependencyRule("N3", "N4"));
        }

        [TestMethod]
        public void Parse_VisibleMembers()
        {
            var xDocument = LoadXml("VisibleMembers.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var visibleTypesByNamespace = config.VisibleTypesByNamespace;
            visibleTypesByNamespace.Should().HaveCount(2);
            {
                var types = visibleTypesByNamespace[new Namespace("N1")];
                types.Should().HaveCount(2);
                types.Should().Contain("T1");
                types.Should().Contain("T2");
            }
            {
                var types = visibleTypesByNamespace[new Namespace("N2")];
                types.Should().HaveCount(1);
                types.Should().Contain("T3");
            }
        }

        [TestMethod]
        public void Parse_AllowedRuleForNamespaceTreeWithVisibleMembers_Throws()
        {
            var xDocument = LoadXml("AllowedRuleForNamespaceTreeWithVisibleMembers.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*must be a single namespace*");
        }

        [TestMethod]
        public void Parse_AllowedRuleForNamespaceWithVisibleMembersWithOfNamespaceAttribute_Throws()
        {
            var xDocument = LoadXml("AllowedRuleForNamespaceWithVisibleMembersWithOfNamespaceAttribute.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*'OfNamespace' attribute must not be defined*");
        }

        [TestMethod]
        public void Parse_VisibleMembersOfNamespaceMissing_Throws()
        {
            var xDocument = LoadXml("VisibleMembersOfNamespaceMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*'OfNamespace' attribute missing*");
        }

        [TestMethod]
        public void Parse_AllowedRuleFromAttributeMissing_Throws()
        {
            var xDocument = LoadXml("AllowedRuleFromAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*'From' attribute missing*");
        }

        [TestMethod]
        public void Parse_AllowedRuleToAttributeMissing_Throws()
        {
            var xDocument = LoadXml("AllowedRuleToAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*'To' attribute missing*");
        }

        [TestMethod]
        public void Parse_VisibleMembersTypeNameAttributeMissing_Throws()
        {
            var xDocument = LoadXml("VisibleMembersTypeNameAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*'Name' attribute missing*");
        }

        [TestMethod]
        public void Parse_NsDepCopConfigElementNotFound_Throws()
        {
            var xDocument = LoadXml("NsDepCopConfigElementNotFound.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*root element not found*");
        }

        [TestMethod]
        public void Parse_InvalidNamespaceString_Throws()
        {
            var xDocument = LoadXml("InvalidNamespaceString.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*not a valid Namespace*");
        }

        [TestMethod]
        public void Parse_InvalidAttributeValue_Throws()
        {
            var xDocument = LoadXml("InvalidAttributeValue.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.ShouldThrow<Exception>().WithMessage("*Error parsing 'IsEnabled' value*");
        }

        private XDocument LoadXml(string filename)
        {
            var path = GetTestFilePath(filename);
            return XDocument.Load(path);
        }
    }
}
