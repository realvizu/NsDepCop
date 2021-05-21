using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Analyzer.Test.Interface.Config
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

        private static int MatchRelevance(string namespaceSpecificationAsString, string namespaceAsString)
        {
            var ns = new Namespace(namespaceAsString);
            return NamespaceSpecificationParser.Parse(namespaceSpecificationAsString).GetMatchRelevance(ns);
        }
    }
}
