using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
{
    public class CachingTypeDependencyValidatorTests
    {
        [Fact]
        public void NoRule_SameNamespaceIsAlwaysAllowed()
        {
            var ruleConfig = new DependencyRulesBuilder();

            var cachingTypeDependencyValidator = CreateCachingTypeDependencyValidator(ruleConfig);
            cachingTypeDependencyValidator.IsAllowedDependency("N", "C1", "N", "C2").Should().BeTrue();
        }

        [Fact]
        public void ValidatingATypeDependencyTwice_FirstCacheMissThenCacheHit()
        {
            var ruleConfig = new DependencyRulesBuilder();

            var cachingTypeDependencyValidator = CreateCachingTypeDependencyValidator(ruleConfig);

            cachingTypeDependencyValidator.IsAllowedDependency("N1", "C1", "N2", "C2").Should().BeFalse();
            cachingTypeDependencyValidator.MissCount.Should().Be(1);
            cachingTypeDependencyValidator.HitCount.Should().Be(0);

            cachingTypeDependencyValidator.IsAllowedDependency("N1", "C1", "N2", "C2").Should().BeFalse();
            cachingTypeDependencyValidator.MissCount.Should().Be(1);
            cachingTypeDependencyValidator.HitCount.Should().Be(1);
        }

        private static CachingTypeDependencyValidator CreateCachingTypeDependencyValidator(IDependencyRules ruleConfig)
            => new CachingTypeDependencyValidator(ruleConfig, traceMessageHandler: null);
    }
}
