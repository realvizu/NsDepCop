using System;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Interface.Config
{
    public class DependencyRulePlaceholderTests
    {
        [Fact]
        public void Matches_WithoutPlaceholders_BehavesLikeIndependentSideMatching()
        {
            var rule = new DependencyRule("S.*", "T.*");

            rule.Matches(new Domain("S.A"), new Domain("T.B")).Should().BeTrue();
            rule.Matches(new Domain("S.A"), new Domain("X")).Should().BeFalse();
            rule.Matches(new Domain("X"), new Domain("T.B")).Should().BeFalse();
        }

        [Fact]
        public void Matches_TransfersCaptureFromFromSideToToSide()
        {
            var rule = new DependencyRule("MyApp.[Module].Domain", "MyApp.[Module].Contracts");

            // Same module: allowed.
            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.A.Contracts")).Should().BeTrue();
            rule.Matches(new Domain("MyApp.B.Domain"), new Domain("MyApp.B.Contracts")).Should().BeTrue();

            // Cross module: the capture does not match the target.
            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.B.Contracts")).Should().BeFalse();

            // 'From' side does not match at all.
            rule.Matches(new Domain("Other.A.Domain"), new Domain("MyApp.A.Contracts")).Should().BeFalse();
        }

        [Fact]
        public void Matches_SubstitutedToSide_MayStillContainWildcards()
        {
            var rule = new DependencyRule("MyApp.[Module].Domain", "MyApp.[Module].Contracts.*");

            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.A.Contracts")).Should().BeTrue();
            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.A.Contracts.Events")).Should().BeTrue();
            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.B.Contracts.Events")).Should().BeFalse();
        }

        [Fact]
        public void Matches_MultiComponentPlaceholder_TransfersWholeSubPath()
        {
            var rule = new DependencyRule("App.[Path*].Domain", "App.[Path*].Contracts");

            rule.Matches(new Domain("App.A.B.Domain"), new Domain("App.A.B.Contracts")).Should().BeTrue();
            rule.Matches(new Domain("App.A.B.Domain"), new Domain("App.A.Contracts")).Should().BeFalse();
        }

        [Fact]
        public void Matches_PlaceholderOnFromSideOnly_MatchesToSideIndependently()
        {
            var rule = new DependencyRule("MyApp.[Module].Domain", "MyApp.Common");

            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.Common")).Should().BeTrue();
            rule.Matches(new Domain("MyApp.A.Domain"), new Domain("MyApp.Other")).Should().BeFalse();
        }

        [Fact]
        public void Matches_PlaceholdersAreBoundByName_NotByPosition()
        {
            var rule = new DependencyRule("App.[P1].[P2].*", "App.[P2].[P1].*");

            rule.Matches(new Domain("App.X.Y.Domain"), new Domain("App.Y.X.Contracts")).Should().BeTrue();
            rule.Matches(new Domain("App.X.Y.Domain"), new Domain("App.X.Y.Contracts")).Should().BeFalse();
        }

        [Fact]
        public void Matches_SameCaptureUsedTwiceOnToSide_Works()
        {
            var rule = new DependencyRule("A.[M].B", "X.[M].Y.[M]");

            rule.Matches(new Domain("A.Foo.B"), new Domain("X.Foo.Y.Foo")).Should().BeTrue();
            rule.Matches(new Domain("A.Foo.B"), new Domain("X.Foo.Y.Bar")).Should().BeFalse();
        }

        [Fact]
        public void Create_UnboundPlaceholderOnToSide_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("MyApp.[Module].Domain", "MyApp.[Other].Contracts"));
        }

        [Fact]
        public void Create_ToSidePlaceholderWithoutPlaceholderFromSide_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("MyApp.*", "MyApp.[Module].Contracts"));
        }

        [Fact]
        public void Create_DuplicatePlaceholderNamesOnFromSide_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("MyApp.[M].[M]", "MyApp.[M]"));
        }

        [Fact]
        public void Matches_NegatedPlaceholder_ExpressesInequality()
        {
            var rule = new DependencyRule("Features.[F].*", "Features.[!F].Shared.*");

            // F != G: the rule fires.
            rule.Matches(new Domain("Features.A.Domain"), new Domain("Features.B.Shared.Events")).Should().BeTrue();

            // F == G: the rule does not fire.
            rule.Matches(new Domain("Features.A.Domain"), new Domain("Features.A.Shared.Events")).Should().BeFalse();

            // Different 'To' shape: the rule does not fire.
            rule.Matches(new Domain("Features.A.Domain"), new Domain("Features.Shared.Events")).Should().BeFalse();
        }

        [Fact]
        public void Matches_NegatedAndPositiveReference_Combined()
        {
            var rule = new DependencyRule("App.[F].[Layer].*", "App.[!F].[Layer].*");

            // Other feature, same layer: fires.
            rule.Matches(new Domain("App.A.Domain"), new Domain("App.B.Domain")).Should().BeTrue();

            // Other feature, other layer: does not fire.
            rule.Matches(new Domain("App.A.Domain"), new Domain("App.B.Infrastructure")).Should().BeFalse();

            // Same feature: does not fire.
            rule.Matches(new Domain("App.A.Domain"), new Domain("App.A.Domain")).Should().BeFalse();
        }

        [Fact]
        public void Create_NegatedPlaceholderOnFromSide_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("Features.[!F].*", "Features.Shared.*"));
        }

        [Fact]
        public void Create_UnboundNegatedPlaceholder_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("Features.[F].*", "Features.[!G].Shared.*"));
        }

        [Fact]
        public void Create_NegatedReferenceToMultiPlaceholder_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => new DependencyRule("Features.[F*].X", "Features.[!F].Shared.*"));
        }

        [Fact]
        public void GetFromMatchRelevance_PlaceholderRule_EqualsDegradedWildcardRelevance()
        {
            var rule = new DependencyRule("MyApp.[Module].Domain", "MyApp.[Module].Contracts");
            var from = new Domain("MyApp.A.Domain");

            rule.GetFromMatchRelevance(from)
                .Should().Be(new WildcardDomain("MyApp.?.Domain").GetMatchRelevance(from));
        }

        [Fact]
        public void EqualsAndHashCode_WorkAsDictionaryKey_ForPlaceholderRules()
        {
            var rule1 = new DependencyRule("MyApp.[M].Domain", "MyApp.[M].Contracts");
            var rule2 = new DependencyRule("MyApp.[M].Domain", "MyApp.[M].Contracts");
            var rule3 = new DependencyRule("MyApp.[M].Domain", "MyApp.[M].Application");

            rule1.Equals(rule2).Should().BeTrue();
            rule1.GetHashCode().Should().Be(rule2.GetHashCode());
            rule1.Equals(rule3).Should().BeFalse();
        }
    }
}
