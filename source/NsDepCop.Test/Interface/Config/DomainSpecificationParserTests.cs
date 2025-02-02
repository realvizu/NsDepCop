using System;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class DomainSpecificationParserTests
    {
        [Theory]
        [InlineData(".")]
        [InlineData("A.B")]
        public void Parse_DomainSpecification(string domainString)
        {
            DomainSpecificationParser.Parse(domainString).Should().Be(new Domain(domainString));
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
        public void Parse_WildcardDomainSpecification(string wildcardDomainString)
        {
            DomainSpecificationParser.Parse(wildcardDomainString).Should().Be(new WildcardDomain(wildcardDomainString));
        }

        [Theory]
        [InlineData("/Unit\\.Test/")]
        [InlineData("/^Unit\\.Test$/")]
        public void Parse_RegexDomainSpecification(string regexDomainString)
        {
            DomainSpecificationParser.Parse(regexDomainString).Should().Be(new RegexDomain(regexDomainString));
        }

        [Theory]
        [InlineData("..")]
        [InlineData(".A")]
        [InlineData("A.")]
        [InlineData("*.*")]
        [InlineData("/foo{2,1}/")]
        public void Parse_Invalid(string invalidString)
        {
            Assert.Throws<FormatException>(() => DomainSpecificationParser.Parse(invalidString));
        }
    }
}