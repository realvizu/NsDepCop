using System;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class NamespaceSpecificationParserTests
    {
        [Theory]
        [InlineData(".")]
        [InlineData("A.B")]
        public void Parse_Namespace(string namespaceString)
        {
            NamespaceSpecificationParser.Parse(namespaceString).Should().Be(new Namespace(namespaceString));
        }

        [Theory]
        [InlineData("*")]
        [InlineData("A.*")]
        [InlineData("A.*.B")]
        [InlineData("A.?.B")]
        [InlineData("*.B")]
        [InlineData("?.B")]
        [InlineData("A.B.?")]
        [InlineData("A.B.*")]
        public void Parse_WildcardNamespace(string wildcardNamespaceString)
        {
            NamespaceSpecificationParser.Parse(wildcardNamespaceString).Should().Be(new WildcardNamespace(wildcardNamespaceString));
        }

        [Theory]
        [InlineData("..")]
        [InlineData(".A")]
        [InlineData("A.")]
        [InlineData("*.*")]
        public void Parse_Invalid(string invalidString)
        {
            Assert.Throws<FormatException>(() => NamespaceSpecificationParser.Parse(invalidString));
        }
    }
}
