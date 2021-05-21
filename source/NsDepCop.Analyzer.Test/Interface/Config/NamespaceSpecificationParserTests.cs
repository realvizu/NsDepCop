using System;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Analyzer.Test.Interface.Config
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
        public void Parse_NamespaceTree(string namespaceTreeString)
        {
            NamespaceSpecificationParser.Parse(namespaceTreeString).Should().Be(new NamespaceTree(namespaceTreeString));
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
