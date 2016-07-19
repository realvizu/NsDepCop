using Codartis.NsDepCop.Core.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Implementation
{
    [TestClass]
    public class NamespaceSpecificationTests
    {
        [TestMethod]
        public void GetMatchRelevance()
        {
            const string ns = "A.B";

            MatchRelevance("D", ns).ShouldEqual(0);
            MatchRelevance("D.*", ns).ShouldEqual(0);
            MatchRelevance("A.B.C.*", ns).ShouldEqual(0);
            MatchRelevance("A", ns).ShouldEqual(0);
            MatchRelevance(".", ns).ShouldEqual(0);

            MatchRelevance("*", ns).ShouldBeGreaterThan(0);
            MatchRelevance("A.*", ns).ShouldBeGreaterThan(MatchRelevance("*", ns));
            MatchRelevance("A.B.*", ns).ShouldBeGreaterThan(MatchRelevance("A.*", ns));
            MatchRelevance("A.B", ns).ShouldBeGreaterThan(MatchRelevance("A.B.*", ns));
        }

        private static int MatchRelevance(string namespaceSpecificationAsString, string namespaceAsString)
        {
            var ns = new Namespace(namespaceAsString);
            return NamespaceSpecificationParser.Parse(namespaceSpecificationAsString).GetMatchRelevance(ns);
        }
    }
}
