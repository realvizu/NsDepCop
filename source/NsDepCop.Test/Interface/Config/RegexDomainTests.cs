using System;
using System.Diagnostics.CodeAnalysis;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config;

public sealed class RegexDomainTests
{
    [Theory]
    [InlineData("/Unit\\.Test/")]
    [InlineData("/^Unit\\.Test(\\.[A-Za-z_][A-Za-z0-9_]*)*$/")]
    public void Create_Works(string regexDomainString)
    {
        new RegexDomain(regexDomainString).ToString().Should().Be(regexDomainString);
    }

    [Fact]
    public void Create_WithNull_ThrowsArgumentNullException()
    {
        var act = () => new RegexDomain(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("//")]
    [InlineData("^Unit\\.Test(\\.[A-Za-z_][A-Za-z0-9_]*)*$")]
    [InlineData("/foo{2,1}/")]
    [InlineData("/(abc\\Kdef)/")]
    [InlineData("/((a|b|)/")]
    public void Create_InvalidRegexDomain_ThrowsFormatException(string regexDomainString)
    {
        var act = () => new RegexDomain(regexDomainString);
        act.Should().Throw<FormatException>();
    }

    [Fact]
    [SuppressMessage("ReSharper", "EqualExpressionComparison")]
    public void Equals_Works()
    {
        (new RegexDomain("/Unit\\.Test/") == new RegexDomain("/Unit\\.Test/")).Should().BeTrue();
        (new RegexDomain("/Unit\\.Test/") == new RegexDomain("/^Unit\\.Test$/")).Should().BeFalse();
    }

    [Fact]
    public void GetMatchRelevance_WhenTimeoutOccurs_ShouldReturnNoMatch_WithoutThrowingException()
    {
        var rule = new RegexDomain("/(a|aa)+$/", regexTimeout: TimeSpan.FromTicks(1));
        var domain = new Domain(new string('a', 10));
        var action = () => rule.GetMatchRelevance(domain);
        action.Should().NotThrow();
    }
}