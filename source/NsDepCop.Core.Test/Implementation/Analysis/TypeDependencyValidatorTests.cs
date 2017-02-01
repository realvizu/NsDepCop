using Codartis.NsDepCop.Core.Implementation.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
{
    [TestClass]
    public class TypeDependencyValidatorTests
    {
        [TestMethod]
        public void NoRule_SameNamespaceIsAlwaysAllowed()
        {
            var ruleConfig = new RuleConfigBuilder();

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N", "C1", "N", "C2").ShouldBeTrue();
        }

        [TestMethod]
        public void NoRule_EverythingIsDisallowed_ExceptSameNamespace()
        {
            var ruleConfig = new RuleConfigBuilder();

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1", "C1", "N2", "C2").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowRule()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("S", "T");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowRule_WithSubnamespace()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("S.*", "T.*");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S.S1", "C1", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S", "C1", "T.T1", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowRule_WithAnyNamespace()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("*", "*");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C1", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C1", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S", "C1", "T1", "C2").ShouldBeTrue();
        }

        [TestMethod]
        public void AllowRuleWithVisibleMembers_AffectsOnlyAllowRuleSource()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("S1", "T", "C1", "C2")
                .AddAllowed("S2", "T");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C1").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C3").ShouldBeFalse();

            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C1").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C3").ShouldBeTrue();
        }

        [TestMethod]
        public void AllowRule_GlobalVisibleMembers_AffectsAllRuleSources()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("S1", "T")
                .AddAllowed("S2", "T")
                .AddVisibleMembers("T", "C1", "C2");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C1").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S1", "C", "T", "C3").ShouldBeFalse();

            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C1").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C2").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("S2", "C", "T", "C3").ShouldBeFalse();
        }

        [TestMethod]
        public void DisallowRule()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddDisallowed("S", "T");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C", "T", "C1").ShouldBeFalse();
        }

        [TestMethod]
        public void DisallowRule_StrongerThanAllowRule()
        {
            var ruleConfig = new RuleConfigBuilder()
                .AddAllowed("S", "T")
                .AddDisallowed("S", "T");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("S", "C", "T", "C1").ShouldBeFalse();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly()
        {
            var ruleConfig = new RuleConfigBuilder()
                .SetChildCanDependOnParentImplicitly(true);

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").ShouldBeFalse();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly_ButDisallowWins()
        {
            var ruleConfig = new RuleConfigBuilder()
                .SetChildCanDependOnParentImplicitly(true)
                .AddDisallowed("N1.N2", "N1");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").ShouldBeFalse();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly_ButDisallowWins_WithWildcard()
        {
            var ruleConfig = new RuleConfigBuilder()
                .SetChildCanDependOnParentImplicitly(true)
                .AddDisallowed("N1.*", "N1");

            var dependencyValidator = new TypeDependencyValidator(ruleConfig);
            dependencyValidator.IsAllowedDependency("N1.N2", "C", "N1", "C1").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("N1", "C", "N1.N2", "C1").ShouldBeFalse();
        }
    }
}
