using System.Linq;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class DomainSpecificationTests
    {
        [Theory]
        [InlineData("A.B", "D", 0)]
        [InlineData("A.B", "D.*", 0)]
        [InlineData("A.B", "A.B.C.*", 0)]
        [InlineData("A.B", "A", 0)]
        [InlineData("A.B", ".", 0)]
        [InlineData("A.B", "A.B", int.MaxValue)]
        [InlineData("A.B", "A.?", int.MaxValue - 1)]
        [InlineData("A.B", "A.*", int.MaxValue - 2)]
        [InlineData("A.B", "A.B.*", int.MaxValue - 1)]
        [InlineData("A.B", "A.B.?", 0)]
        [InlineData("A.B", "/A\\.B/", 1)]
        [InlineData("A.B", "/A\\../", 1)]
        [InlineData("A.B", "/A\\.[A-Z]/", 1)]
        [InlineData("A.B", "/A\\.[0-9]/", 0)]
        [InlineData("A.C.D", "B.?.D", 0)]
        [InlineData("A.C.D", "*.E", 0)]
        [InlineData("A.C.D", "A.C.D", int.MaxValue)]
        [InlineData("A.C.D", "A.?.D", int.MaxValue - 1)]
        [InlineData("A.C.D", "A.C.?", int.MaxValue - 1)]
        [InlineData("A.C.D", "?.C.D", int.MaxValue - 1)]
        [InlineData("A.C.D", "A.?.?", int.MaxValue - 2)]
        [InlineData("A.C.D", "?.?.D", int.MaxValue - 2)]
        [InlineData("A.C.D", "?.?.?", int.MaxValue - 3)]
        [InlineData("A.C.D", "A.*", int.MaxValue - 3)]
        [InlineData("A.C.D", "*.D", int.MaxValue - 3)]
        [InlineData("A.C.D", "*.?.D", int.MaxValue - 3)]
        [InlineData("A.B.C.D", "*.?.D", int.MaxValue - 4)]
        [InlineData("A.C.D", "*", int.MaxValue - 4)]
        [InlineData("A.F1.B.C", "A.*.B", 0)]
        [InlineData("A.F1.B.C", "A.*.B.*", int.MaxValue - 4)]
        [InlineData("A.F1.B.C", "A.*.B.?", int.MaxValue - 3)]
        [InlineData("A.F1.B.C", "A.?.B.?", int.MaxValue - 2)]
        [InlineData("A.F1.B.C", "A.?.B.*", int.MaxValue - 3)]
        public void GetMatchRelevance_ShouldReturnTheExpectedValue(string domainString, string domainSpecificationString, int expectedMatchRelevance)
        {
            var domain = new Domain(domainString);
            var domainSpecification = DomainSpecificationParser.Parse(domainSpecificationString);
            domainSpecification.GetMatchRelevance(domain).Should().Be(expectedMatchRelevance);
        }

        /// <summary>
        /// This test verifies that the matching rules are prioritized correctly, according to their match relevance.
        /// </summary>
        [Theory]
        [InlineData("A.B", "A.B", "A.B.*", "A.?", "A.*", "/A\\.B/")]
        public void GetMatchRelevance_ShouldPrioritizeProperly(string domainString, params string[] domainSpecificationStrings)
        {
            var domain = new Domain(domainString);
            var domainSpecifications = domainSpecificationStrings.Select(DomainSpecificationParser.Parse);
            domainSpecifications.Select(i => i.GetMatchRelevance(domain)).Should().BeInDescendingOrder();
        }
    }
}