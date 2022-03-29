namespace Codartis.NsDepCop.Test.Implementation
{
    using System.Linq;
    using Codartis.NsDepCop.Config;
    using FluentAssertions;
    using Xunit;
    public class ExtendedSystaxDistanceTests
    {
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
        public void ExpectedDistance(string actual, string pattern, int distance)
        {
            NamespaceSpecification patternNs = pattern.Any(s => s == '*' || s == '?') ? new WildcardNamespace(pattern) : new Namespace(pattern);
            var ns = new Namespace(actual);

            patternNs.GetMatchRelevance(ns).Should().Be(int.MaxValue - distance);
        }

    }
}