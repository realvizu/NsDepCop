using System;
using System.Xml.Linq;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Config.Implementation;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Config
{
    public class XmlConfigParserTests : FileBasedTestsBase
    {
        [Fact]
        public void Parse_NoRootAttributes()
        {
            var xDocument = LoadXml("NoRootAttributes.nsdepcop");
            var configBuilder = XmlConfigParser.Parse(xDocument);
            configBuilder.IsEnabled.Should().BeNull();
            configBuilder.InheritanceDepth.Should().BeNull();
            configBuilder.MaxIssueCount.Should().BeNull();
            configBuilder.ChildCanDependOnParentImplicitly.Should().BeNull();
            configBuilder.AutoLowerMaxIssueCount.Should().BeNull();
            configBuilder.SourcePathExclusionPatterns.Should().BeEmpty();
            configBuilder.CheckAssemblyDependencies.Should().BeNull();
        }

        [Fact]
        public void Parse_RootAttributes()
        {
            var xDocument = LoadXml("RootAttributes.nsdepcop");
            var configBuilder = XmlConfigParser.Parse(xDocument);
            configBuilder.IsEnabled.Should().BeTrue();
            configBuilder.InheritanceDepth.Should().Be(9);
            configBuilder.MaxIssueCount.Should().Be(42);
            configBuilder.ChildCanDependOnParentImplicitly.Should().BeTrue();
            configBuilder.AutoLowerMaxIssueCount.Should().BeTrue();
            configBuilder.SourcePathExclusionPatterns.Should().BeEquivalentTo(@"**/*.g.cs", @"TestData\**\*.cs");
            configBuilder.CheckAssemblyDependencies.Should().BeTrue();
        }

        [Fact]
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

        [Fact]
        public void Parse_DisallowedRules()
        {
            var xDocument = LoadXml("DisallowedRules.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var disallowedRules = config.DisallowRules;
            disallowedRules.Should().HaveCount(2);

            disallowedRules.Should().Contain(new NamespaceDependencyRule("N1", "N2"));
            disallowedRules.Should().Contain(new NamespaceDependencyRule("N3", "N4"));
        }

        [Fact]
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

        [Fact]
        public void Parse_AllowedAssemblyRules()
        {
            var xDocument = LoadXml("AllowedAssemblyRules.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var allowedAssemblyRules = config.AllowedAssemblyRules;
            allowedAssemblyRules.Should().HaveCount(2);

            allowedAssemblyRules.Should().Contain(new NamespaceDependencyRule("A1", "A2"));
            allowedAssemblyRules.Should().Contain(new NamespaceDependencyRule("A3", "A4"));
        }

        [Fact]
        public void Parse_DisallowedAssemblyRules()
        {
            var xDocument = LoadXml("DisallowedAssemblyRules.nsdepcop");
            var config = XmlConfigParser.Parse(xDocument);

            var disallowedAssemblyRules = config.DisallowedAssemblyRules;
            disallowedAssemblyRules.Should().HaveCount(2);

            disallowedAssemblyRules.Should().Contain(new NamespaceDependencyRule("A1", "A2"));
            disallowedAssemblyRules.Should().Contain(new NamespaceDependencyRule("A3", "A4"));
        }

        [Fact]
        public void Parse_AllowedRuleForWildcardNamespaceWithVisibleMembers_Throws()
        {
            var xDocument = LoadXml("AllowedRuleForWildcardNamespaceWithVisibleMembers.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*must be a single namespace*");
        }

        [Fact]
        public void Parse_AllowedRuleForNamespaceWithVisibleMembersWithOfNamespaceAttribute_Throws()
        {
            var xDocument = LoadXml("AllowedRuleForNamespaceWithVisibleMembersWithOfNamespaceAttribute.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*'OfNamespace' attribute must not be defined*");
        }

        [Fact]
        public void Parse_VisibleMembersOfNamespaceMissing_Throws()
        {
            var xDocument = LoadXml("VisibleMembersOfNamespaceMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*'OfNamespace' attribute missing*");
        }

        [Fact]
        public void Parse_AllowedRuleFromAttributeMissing_Throws()
        {
            var xDocument = LoadXml("AllowedRuleFromAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*'From' attribute missing*");
        }

        [Fact]
        public void Parse_AllowedRuleToAttributeMissing_Throws()
        {
            var xDocument = LoadXml("AllowedRuleToAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*'To' attribute missing*");
        }

        [Fact]
        public void Parse_VisibleMembersTypeNameAttributeMissing_Throws()
        {
            var xDocument = LoadXml("VisibleMembersTypeNameAttributeMissing.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*'Name' attribute missing*");
        }

        [Fact]
        public void Parse_NsDepCopConfigElementNotFound_Throws()
        {
            var xDocument = LoadXml("NsDepCopConfigElementNotFound.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*root element not found*");
        }

        [Fact]
        public void Parse_InvalidNamespaceString_Throws()
        {
            var xDocument = LoadXml("InvalidNamespaceString.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*not a valid Namespace*");
        }

        [Fact]
        public void Parse_InvalidDuplicatedWildcardNamespaceString_Throws()
        {
            var xDocument = LoadXml("InvalidDuplicatedWildcardNamespaceString.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*not a valid WildcardNamespace*");
        }

        [Fact]
        public void Parse_InvalidAttributeValue_Throws()
        {
            var xDocument = LoadXml("InvalidAttributeValue.nsdepcop");
            Action a = () => XmlConfigParser.Parse(xDocument);
            a.Should().Throw<Exception>().WithMessage("*Error parsing 'IsEnabled' value*");
        }

        [Fact]
        public void UpdateMaxIssueCount_HasMaxIssueCountAttribute_Works()
        {
            const int newMaxIssueCount = 101;
            var xDocument = LoadXml("RootAttributes.nsdepcop");
            XmlConfigParser.UpdateMaxIssueCount(xDocument, newMaxIssueCount);
            xDocument.Element("NsDepCopConfig")?.Attribute("MaxIssueCount")?.Value.Should().Be(newMaxIssueCount.ToString());
        }

        [Fact]
        public void UpdateMaxIssueCount_NoMaxIssueCountAttribute_Works()
        {
            const int newMaxIssueCount = 101;
            var xDocument = LoadXml("NoRootAttributes.nsdepcop");
            XmlConfigParser.UpdateMaxIssueCount(xDocument, newMaxIssueCount);
            xDocument.Element("NsDepCopConfig")?.Attribute("MaxIssueCount")?.Value.Should().Be(newMaxIssueCount.ToString());
        }

        private XDocument LoadXml(string filename)
        {
            var path = GetFilePathInTestClassFolder(filename);
            return XDocument.Load(path);
        }
    }
}