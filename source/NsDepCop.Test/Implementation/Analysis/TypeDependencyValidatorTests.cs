using Codartis.NsDepCop.Implementation.Analysis;
using Codartis.NsDepCop.Interface.Config;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    public class TypeDependencyValidatorTests
    {
        [Fact]
        public void NoRule_SameNamespaceIsAlwaysAllowed()
        {
            var ruleConfig = new DependencyRulesBuilder();

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N", "C1", "N", "C2").Should().BeTrue();
        }

        [Fact]
        public void NoRule_EverythingIsDisallowed_ExceptSameNamespace()
        {
            var ruleConfig = new DependencyRulesBuilder();

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1", "C1", "N2", "C2").Should().BeFalse();
        }

        [Fact]
        public void AllowRule()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("S", "T");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").Should().BeFalse();
        }

        [Fact]
        public void AllowRule_WithSubnamespace()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("S.*", "T.*");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S.S1", "C1", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S", "C1", "T.T1", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").Should().BeFalse();
        }

        [Fact]
        public void AllowRule_WithAnyNamespace()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("*", "*");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").Should().BeTrue();
        }

        [Fact]
        public void AllowRuleWithVisibleMembers_AffectsOnlyAllowRuleSource()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("S1", "T", "C1", "C2")
                .AddAllowed("S2", "T");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C1").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C3").Should().BeFalse();

            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C1").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C3").Should().BeTrue();
        }

        [Fact]
        public void AllowRule_GlobalVisibleMembers_AffectsAllRuleSources()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("S1", "T")
                .AddAllowed("S2", "T")
                .AddVisibleMembers("T", "C1", "C2");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C1").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C3").Should().BeFalse();

            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C1").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C3").Should().BeFalse();
        }

        [Fact]
        public void DisallowRule()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddDisallowed("S", "T");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C", "T", "C1").Should().BeFalse();
        }

        [Fact]
        public void DisallowRule_StrongerThanAllowRule()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("S", "T")
                .AddDisallowed("S", "T");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C", "T", "C1").Should().BeFalse();
        }

        [Fact]
        public void DisallowRule_ButSameNamespaceShouldBeAllowed()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddDisallowed("a.*", "a.b.*");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("a.b.c", "C1", "a.b.c", "C2").Should().BeTrue();
        }

        [Fact]
        public void DisallowRule_IsStrongerThanChildCanDependOnParentImplicitly()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddDisallowed("a.*", "a.b.*")
                .SetChildCanDependOnParentImplicitly(true);

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("a.b.c", "C1", "a.b", "C2").Should().BeFalse();
        }

        [Fact]
        public void ChildCanDependOnParentImplicitly()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .SetChildCanDependOnParentImplicitly(true);

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").Should().BeFalse();
        }

        [Fact]
        public void ChildCanDependOnParentImplicitly_ButDisallowWins()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .SetChildCanDependOnParentImplicitly(true)
                .AddDisallowed("N1.N2", "N1");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").Should().BeFalse();
        }

        [Fact]
        public void ChildCanDependOnParentImplicitly_ButDisallowWins_WithWildcard()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .SetChildCanDependOnParentImplicitly(true)
                .AddDisallowed("N1.*", "N1");

            var dependencyValidator = CreateTypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").Should().BeFalse();
        }

        private static TypeDependencyValidator CreateTypeDependencyValidator(IDependencyRules ruleConfig) => new TypeDependencyValidator(ruleConfig);
    }
}
