using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Analyzer.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;

namespace Codartis.NsDepCop.Core.Test.Analyzer.Common
{
    [TestClass]
    public class NamespaceDependencyValidatorTests
    {
        [TestMethod]
        public void NoRule_SameIsAllowed()
        {
            var allowedDependencies = ImmutableHashSet.Create<Dependency>();
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency(".", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A.B").ShouldBeTrue();
        }

        [TestMethod]
        public void NoRule_EverythingIsDisallowed()
        {
            var allowedDependencies = ImmutableHashSet.Create<Dependency>();
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "A.B").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowedRule_ConcreteMatch()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("A", "B"));
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeTrue();

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "C").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "A.B").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowedRule_WildcardMatch()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("*", "*"));
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("B", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("B", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "A.B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A.C").ShouldBeTrue();
        }

        [TestMethod]
        public void AllowedRule_SubnamespaceWildcardMatch()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("A.*", "B.*"));
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "B.A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "B.A").ShouldBeTrue();

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "C").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "A.B").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B", ".").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B", "C").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B", "A.C").ShouldBeFalse();
        }

        [TestMethod]
        public void DisallowedRule_ConcreteMatch()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("*", "*"));
            var disallowedDependencies = ImmutableHashSet.Create(new Dependency("A", "B"));
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "B.A").ShouldBeTrue();

            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeFalse();
        }

        [TestMethod]
        public void DisallowedRule_WildcardMatch()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("*", "*"));
            var disallowedDependencies = ImmutableHashSet.Create(new Dependency("A.*", "B.*"));
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A", "C").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "C.A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency(".", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("C", "B").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("C", "B.A").ShouldBeTrue();

            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B", "B").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A", "B.A").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B", "B.A").ShouldBeFalse();
        }

        [TestMethod]
        public void AllowedAndDisallowedRule_DisallowedWins()
        {
            var allowedDependencies = ImmutableHashSet.Create(new Dependency("A", "B"));
            var disallowedDependencies = ImmutableHashSet.Create(new Dependency("A", "B"));
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies);

            dependencyValidator.IsAllowedDependency("A", "B").ShouldBeFalse();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly()
        {
            var allowedDependencies = ImmutableHashSet.Create<Dependency>();
            var disallowedDependencies = ImmutableHashSet.Create<Dependency>();
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies, true);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B.C", "A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B.C", "A.B").ShouldBeTrue();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly_ButDisallowedWins()
        {
            var allowedDependencies = ImmutableHashSet.Create<Dependency>();
            var disallowedDependencies = ImmutableHashSet.Create(new Dependency("A.B", "A"));
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies, true);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B.C", "A").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B.C", "A.B").ShouldBeTrue();
        }

        [TestMethod]
        public void ChildCanDependOnParentImplicitly_ButDisallowedWithWildcardsWins()
        {
            var allowedDependencies = ImmutableHashSet.Create<Dependency>();
            var disallowedDependencies = ImmutableHashSet.Create(new Dependency("A.*", "A"));
            var dependencyValidator = new NamespaceDependencyValidator(allowedDependencies, disallowedDependencies, true);

            dependencyValidator.IsAllowedDependency("A", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", ".").ShouldBeTrue();
            dependencyValidator.IsAllowedDependency("A.B", "A").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B.C", "A").ShouldBeFalse();
            dependencyValidator.IsAllowedDependency("A.B.C", "A.B").ShouldBeTrue();
        }
    }
}
