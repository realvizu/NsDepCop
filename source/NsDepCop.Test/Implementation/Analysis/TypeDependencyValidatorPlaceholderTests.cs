using Codartis.NsDepCop.Analysis.Implementation;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    public class TypeDependencyValidatorPlaceholderTests
    {
        [Fact]
        public void AllowRule_WithPlaceholder_AllowsOnlyWithinTheSameModule()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("MyApp.[Module].Domain", "MyApp.[Module].Contracts");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.A.Contracts", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("MyApp.B.Domain", "C1", "MyApp.B.Contracts", "C2").Should().BeTrue();

            // The capture links both sides: cross-module access is disallowed.
            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.B.Contracts", "C2").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("MyApp.B.Domain", "C1", "MyApp.A.Contracts", "C2").Should().BeFalse();
        }

        [Fact]
        public void AllowRule_WithPlaceholderAndTrailingWildcard_AllowsModuleSubtree()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("MyApp.[Module].Application.*", "MyApp.[Module].Contracts.*");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            dependencyValidator.IsAllowedDependency("MyApp.A.Application", "C1", "MyApp.A.Contracts.Events", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("MyApp.A.Application.Handlers", "C1", "MyApp.A.Contracts", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("MyApp.A.Application", "C1", "MyApp.B.Contracts.Events", "C2").Should().BeFalse();
        }

        [Fact]
        public void DisallowRule_WithPlaceholder_TrumpsBroadAllowRule()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("*", "*")
                .AddDisallowed("MyApp.[Module].Domain", "MyApp.[Module].Infrastructure");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            // Same module: the placeholder disallow rule fires.
            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.A.Infrastructure", "C2").Should().BeFalse();

            // Cross module: the disallow rule does not fire, the broad allow rule applies.
            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.B.Infrastructure", "C2").Should().BeTrue();
        }

        [Fact]
        public void AllowRule_WithVisibleMembers_WorksForPlaceholderRules()
        {
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("MyApp.[Module].Application", "MyApp.[Module].Contracts", "VisibleType");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            dependencyValidator.IsAllowedDependency("MyApp.A.Application", "C1", "MyApp.A.Contracts", "VisibleType").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("MyApp.A.Application", "C1", "MyApp.A.Contracts", "HiddenType").Should().BeFalse();
        }

        [Fact]
        public void DisallowRule_WithNegatedPlaceholder_BlocksForeignSharedDespiteBroadAllow()
        {
            // The requirement: features may use their own Shared and parent-level Shared,
            // but never the Shared of a foreign feature - robust even against a broad allow
            // rule (eg. inherited from a parent-level config).
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("Features.*", "Features.*")
                .AddDisallowed("Features.[F].*", "Features.[!F].Shared.*");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            // Own Shared: the negated disallow rule does not fire, the broad allow applies.
            dependencyValidator.IsAllowedDependency("Features.A.Domain", "C1", "Features.A.Shared", "C2").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("Features.A.Domain", "C1", "Features.A.Shared.Events", "C2").Should().BeTrue();

            // Foreign Shared: blocked, no matter what allow rules exist.
            dependencyValidator.IsAllowedDependency("Features.A.Domain", "C1", "Features.B.Shared", "C2").Should().BeFalse();
            dependencyValidator.IsAllowedDependency("Features.A.Domain", "C1", "Features.B.Shared.Events", "C2").Should().BeFalse();

            // Foreign non-Shared namespaces are unaffected by the disallow rule.
            dependencyValidator.IsAllowedDependency("Features.A.Domain", "C1", "Features.B.Contracts", "C2").Should().BeTrue();
        }

        [Fact]
        public void MostSpecificRuleSelection_TreatsPlaceholderLikeSingleWildcard()
        {
            // Both rules match 'MyApp.A.Domain' -> 'MyApp.A.Contracts'.
            // The placeholder rule ('MyApp.?.Domain' equivalent) is more specific than 'MyApp.*'
            // and restricts visible members; the broader rule must not win.
            var ruleConfig = new DependencyRulesBuilder()
                .AddAllowed("MyApp.[Module].Domain", "MyApp.[Module].Contracts", "VisibleType")
                .AddAllowed("MyApp.*", "MyApp.*");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);

            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.A.Contracts", "VisibleType").Should().BeTrue();
            dependencyValidator.IsAllowedDependency("MyApp.A.Domain", "C1", "MyApp.A.Contracts", "HiddenType").Should().BeFalse();
        }
    }
}
