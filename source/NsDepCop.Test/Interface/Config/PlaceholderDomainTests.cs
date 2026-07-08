using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class PlaceholderDomainTests
    {
        [Theory]
        [InlineData("[M]")]
        [InlineData("A.[M]")]
        [InlineData("A.[M].B")]
        [InlineData("A.[M*].B")]
        [InlineData("[Module_1].[Module_2]")]
        [InlineData("A.[M].?.B")]
        [InlineData("A.[M].*")]
        [InlineData("*.A.[M]")]
        [InlineData("A.[M].[M]")]
        [InlineData("A.[!M].B")]
        [InlineData("A.[M].[!N]")]
        public void Create_Works(string placeholderDomainString)
        {
            new PlaceholderDomain(placeholderDomainString).ToString().Should().Be(placeholderDomainString);
        }

        [Fact]
        public void Create_WithNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PlaceholderDomain(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("A.B")]
        [InlineData("A.*")]
        [InlineData("A.?.B")]
        [InlineData("A.[M")]
        [InlineData("A.M]")]
        [InlineData("A.[]")]
        [InlineData("A.[1M]")]
        [InlineData("A.[M-N]")]
        [InlineData("A.X[M]")]
        [InlineData("A.[M]X")]
        [InlineData("A.[M*].[N*]")]
        [InlineData("A.*.[M*]")]
        [InlineData("A.[M*].*")]
        [InlineData("A..[M]")]
        [InlineData("A.[!M*]")]
        [InlineData("A.[!]")]
        [InlineData("A.[!!M]")]
        public void Create_Invalid_ThrowsFormatException(string placeholderDomainString)
        {
            Assert.Throws<FormatException>(() => new PlaceholderDomain(placeholderDomainString));
        }

        [Fact]
        public void PlaceholderNames_ReturnsNamesInOrder_IncludingDuplicates()
        {
            new PlaceholderDomain("A.[M].B.[N*].[M]").PlaceholderNames
                .Should().Equal("M", "N", "M");
        }

        [Theory]
        // Single-component placeholder.
        [InlineData("MyApp.[M].Domain", "MyApp.A.Domain", true, "M", "A")]
        [InlineData("MyApp.[M].Domain", "MyApp.Domain", false, null, null)]
        [InlineData("MyApp.[M].Domain", "MyApp.A.B.Domain", false, null, null)]
        // Multi-component placeholder captures one or more components.
        [InlineData("MyApp.[M*]", "MyApp.A", true, "M", "A")]
        [InlineData("MyApp.[M*]", "MyApp.A.B", true, "M", "A.B")]
        [InlineData("MyApp.[M*]", "MyApp", false, null, null)]
        [InlineData("[M*].Domain", "MyApp.A.Domain", true, "M", "MyApp.A")]
        [InlineData("A.[M*].B", "A.P.Q.B", true, "M", "P.Q")]
        // Mixed with non-capturing wildcards.
        [InlineData("MyApp.[M].*", "MyApp.X.Y.Z", true, "M", "X")]
        [InlineData("MyApp.[M].*", "MyApp.X", true, "M", "X")]
        [InlineData("*.[M].Domain", "A.B.A.Domain", true, "M", "A")]
        [InlineData("MyApp.[M].?", "MyApp.X.Y", true, "M", "X")]
        [InlineData("MyApp.[M].?", "MyApp.X", false, null, null)]
        public void TryMatch_Works(string pattern, string domain, bool expectedResult, string placeholderName, string expectedCapture)
        {
            var isMatch = new PlaceholderDomain(pattern).TryMatch(new Domain(domain), out var capturedValues);

            isMatch.Should().Be(expectedResult);

            if (expectedResult)
                capturedValues[placeholderName].Should().Be(expectedCapture);
            else
                capturedValues.Should().BeNull();
        }

        [Fact]
        public void TryMatch_MultiplePlaceholders_WithBacktracking()
        {
            var isMatch = new PlaceholderDomain("[M].[N*].C").TryMatch(new Domain("A.B1.B2.C"), out var capturedValues);

            isMatch.Should().BeTrue();
            capturedValues["M"].Should().Be("A");
            capturedValues["N"].Should().Be("B1.B2");
        }

        [Fact]
        public void TryMatch_MultiCapture_IsShortestFirst()
        {
            var isMatch = new PlaceholderDomain("A.[M*].?").TryMatch(new Domain("A.P.Q.R"), out var capturedValues);

            isMatch.Should().BeTrue();
            capturedValues["M"].Should().Be("P.Q");
        }

        [Theory]
        [InlineData("MyApp.[M].Contracts", "A", "MyApp.A.Contracts")]
        [InlineData("MyApp.[M].Contracts.*", "A", "MyApp.A.Contracts.*")]
        [InlineData("[M].X.[M]", "A.B", "A.B.X.A.B")]
        public void Substitute_Works(string pattern, string captureValueForM, string expected)
        {
            var captures = new Dictionary<string, string> { ["M"] = captureValueForM };

            new PlaceholderDomain(pattern).Substitute(captures).Should().Be(expected);
        }

        [Fact]
        public void Substitute_MissingCapture_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(
                () => new PlaceholderDomain("A.[M]").Substitute(new Dictionary<string, string>()));
        }

        [Theory]
        // Positive reference: must equal the bound value.
        [InlineData("MyApp.[M].Contracts", "MyApp.A.Contracts", "A", true)]
        [InlineData("MyApp.[M].Contracts", "MyApp.B.Contracts", "A", false)]
        // Negated reference: must differ from the bound value.
        [InlineData("MyApp.[!M].Contracts", "MyApp.B.Contracts", "A", true)]
        [InlineData("MyApp.[!M].Contracts", "MyApp.A.Contracts", "A", false)]
        // Mixed with wildcards.
        [InlineData("MyApp.[!M].Shared.*", "MyApp.B.Shared.Events", "A", true)]
        [InlineData("MyApp.[!M].Shared.*", "MyApp.A.Shared.Events", "A", false)]
        // Negated reference consumes exactly one component.
        [InlineData("MyApp.[!M].Contracts", "MyApp.Contracts", "A", false)]
        public void Matches_WithBoundValues_Works(string pattern, string domain, string boundValueForM, bool expected)
        {
            var boundValues = new Dictionary<string, string> { ["M"] = boundValueForM };

            new PlaceholderDomain(pattern).Matches(new Domain(domain), boundValues).Should().Be(expected);
        }

        [Fact]
        public void Matches_WithBoundValues_PositiveReferenceToMultiCapture_ConsumesAllComponents()
        {
            var boundValues = new Dictionary<string, string> { ["P"] = "A.B" };

            new PlaceholderDomain("X.[P].Y").Matches(new Domain("X.A.B.Y"), boundValues).Should().BeTrue();
            new PlaceholderDomain("X.[P].Y").Matches(new Domain("X.A.Y"), boundValues).Should().BeFalse();
        }

        [Fact]
        public void Substitute_NegatedPlaceholder_ThrowsInvalidOperationException()
        {
            var captures = new Dictionary<string, string> { ["M"] = "A" };

            Assert.Throws<InvalidOperationException>(() => new PlaceholderDomain("A.[!M]").Substitute(captures));
        }

        [Fact]
        public void GetMatchRelevance_EqualsDegradedWildcardRelevance()
        {
            var domain = new Domain("A.X.B");

            new PlaceholderDomain("A.[M].B").GetMatchRelevance(domain)
                .Should().Be(new WildcardDomain("A.?.B").GetMatchRelevance(domain));

            new PlaceholderDomain("A.[M*]").GetMatchRelevance(domain)
                .Should().Be(new WildcardDomain("A.*").GetMatchRelevance(domain));

            new PlaceholderDomain("A.[!M].B").GetMatchRelevance(domain)
                .Should().Be(new WildcardDomain("A.?.B").GetMatchRelevance(domain));
        }

        [Fact]
        public void DomainSpecificationParser_RoutesPlaceholderStrings_ToPlaceholderDomain()
        {
            DomainSpecificationParser.Parse("A.[M].B").Should().BeOfType<PlaceholderDomain>();
            DomainSpecificationParser.Parse("A.[M*].B").Should().BeOfType<PlaceholderDomain>();
        }

        [Fact]
        public void DomainSpecificationParser_RegexWithCharacterClass_IsNotMisroutedToPlaceholderDomain()
        {
            DomainSpecificationParser.Parse(@"/A\.[XY]\.B/").Should().BeOfType<RegexDomain>();
        }
    }
}
