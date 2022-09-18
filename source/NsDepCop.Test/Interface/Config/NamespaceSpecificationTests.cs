using System.Linq;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class NamespaceSpecificationTests
    {
        [Fact]
        public void GetMatchRelevance()
        {
            const string ns = "A.B";

            MatchRelevance("D", ns).Should().Be(0);
            MatchRelevance("D.*", ns).Should().Be(0);
            MatchRelevance("A.B.C.*", ns).Should().Be(0);
            MatchRelevance("A", ns).Should().Be(0);
            MatchRelevance(".", ns).Should().Be(0);

            MatchRelevance("*", ns).Should().BeGreaterThan(0);
            MatchRelevance("A.*", ns).Should().BeGreaterThan(MatchRelevance("*", ns));
            MatchRelevance("A.B.*", ns).Should().BeGreaterThan(MatchRelevance("A.*", ns));
            MatchRelevance("A.B", ns).Should().BeGreaterThan(MatchRelevance("A.B.*", ns));
        }
        
        [Theory]
        [InlineData("A.C.D", "A.C.D", 0)]
        [InlineData("A.C.D", "A.?.D", 1)]
        [InlineData("A.C.D", "A.C.?", 1)]
        [InlineData("A.C.D", "?.C.D", 1)]
        [InlineData("A.C.D", "A.?.?", 2)]
        [InlineData("A.C.D", "?.?.D", 2)]
        [InlineData("A.C.D", "?.?.?", 3)]
        [InlineData("A.C.D", "A.*", 3)]
        [InlineData("A.C.D", "*.D", 3)]
        [InlineData("A.C.D", "*.?.D", 3)]
        [InlineData("A.B.C.D", "*.?.D", 4)]
        [InlineData("A.C.D", "*", 4)]
        [InlineData("A.C.D", "B.?.D", int.MaxValue)]
        [InlineData("A.C.D", "*.E", int.MaxValue)]
        [InlineData("A.F1.B.C", "A.*.B", int.MaxValue)]
        [InlineData("A.F1.B.C", "A.*.B.*", 4)]
        [InlineData("A.F1.B.C", "A.*.B.?", 3)]
        [InlineData("A.F1.B.C", "A.?.B.?", 2)]
        [InlineData("A.F1.B.C", "A.?.B.*", 3)]
        public void VerifyMatchRelevanceWithWildcardSyntax(string ns, string pattern, int distance)
        {
            MatchRelevance(pattern, ns).Should().Be(int.MaxValue - distance);
        }

        private static int MatchRelevance(string namespaceSpecificationAsString, string namespaceAsString)
        {
            var ns = new Namespace(namespaceAsString);
            return NamespaceSpecificationParser.Parse(namespaceSpecificationAsString).GetMatchRelevance(ns);
        }
    }
}
