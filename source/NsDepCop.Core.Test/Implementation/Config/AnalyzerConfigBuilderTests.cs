using System.Collections.Generic;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class AnalyzerConfigBuilderTests
    {
        [TestMethod]
        public void ToAnalyzerConfig_AppliesDefaults()
        {
            var config = new AnalyzerConfigBuilder().ToAnalyzerConfig();

            config.IsEnabled.Should().Be(ConfigDefaults.IsEnabled);
            config.IssueKind.Should().Be(ConfigDefaults.IssueKind);
            config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
            config.Parser.Should().Be(ConfigDefaults.Parser);
            config.ChildCanDependOnParentImplicitly.Should().Be(ConfigDefaults.ChildCanDependOnParentImplicitly);
            config.MaxIssueCount.Should().Be(ConfigDefaults.MaxIssueCount);
        }

        [TestMethod]
        public void ToAnalyzerConfig_OverridingParserRespected()
        {
            var config = new AnalyzerConfigBuilder()
                .OverrideParser(Parsers.NRefactory)
                .SetParser(Parsers.Roslyn)
                .ToAnalyzerConfig();

            config.Parser.Should().Be(Parsers.NRefactory);
        }

        [TestMethod]
        public void SetMethods_WithNonNullValues_OverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetIssueKind(IssueKind.Error)
                .SetInfoImportance(Importance.High)
                .SetParser(Parsers.NRefactory)
                .SetChildCanDependOnParentImplicitly(true)
                .SetMaxIssueCount(42);

            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.IssueKind.Should().Be(IssueKind.Error);
            configBuilder.InfoImportance.Should().Be(Importance.High);
            configBuilder.Parser.Should().Be(Parsers.NRefactory);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [TestMethod]
        public void SetMethods_WithNullValues_DoNotOverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetIssueKind(IssueKind.Error)
                .SetInfoImportance(Importance.High)
                .SetParser(Parsers.NRefactory)
                .SetChildCanDependOnParentImplicitly(true)
                .SetMaxIssueCount(42);

            configBuilder
                .SetIsEnabled(null)
                .SetIssueKind(null)
                .SetInfoImportance(null)
                .SetParser(null)
                .SetChildCanDependOnParentImplicitly(null)
                .SetMaxIssueCount(null);

            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.IssueKind.Should().Be(IssueKind.Error);
            configBuilder.InfoImportance.Should().Be(Importance.High);
            configBuilder.Parser.Should().Be(Parsers.NRefactory);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [TestMethod]
        public void AddAllowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"))
                .AddAllowRule(new NamespaceDependencyRule("N3", "N4"), new TypeNameSet { "T1", "T2" });

            configBuilder
                .AddAllowRule(new NamespaceDependencyRule("N3", "N4"), new TypeNameSet { "T2", "T3" })
                .AddAllowRule(new NamespaceDependencyRule("N5", "N6"), new TypeNameSet { "T4" });

            configBuilder.AllowRules.ShouldBeEquivalentTo(
                new Dictionary<NamespaceDependencyRule, TypeNameSet>
                {
                    {new NamespaceDependencyRule("N1", "N2"), null},
                    {new NamespaceDependencyRule("N3", "N4"), new TypeNameSet{"T1", "T2", "T3"}},
                    {new NamespaceDependencyRule("N5", "N6"), new TypeNameSet{"T4"}},
                });
        }

        [TestMethod]
        public void AddDisallowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddDisallowRule(new NamespaceDependencyRule("N1", "N2"))
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"));

            configBuilder
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddDisallowRule(new NamespaceDependencyRule("N5", "N6"));

            configBuilder.DisallowRules.ShouldBeEquivalentTo(
                new HashSet<NamespaceDependencyRule>
                {
                    new NamespaceDependencyRule("N1", "N2"),
                    new NamespaceDependencyRule("N3", "N4"),
                    new NamespaceDependencyRule("N5", "N6"),
                });
        }

        [TestMethod]
        public void AddVisibleTypesByNamespace_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddVisibleTypesByNamespace(new Namespace("N1"), null)
                .AddVisibleTypesByNamespace(new Namespace("N2"), new TypeNameSet { "T1", "T2" });

            configBuilder
                .AddVisibleTypesByNamespace(new Namespace("N2"), new TypeNameSet { "T2", "T3" })
                .AddVisibleTypesByNamespace(new Namespace("N3"), new TypeNameSet { "T4" });

            configBuilder.VisibleTypesByNamespace.ShouldBeEquivalentTo(
                new Dictionary<Namespace, TypeNameSet>
                {
                    {new Namespace("N1"), null},
                    {new Namespace("N2"), new TypeNameSet{"T1", "T2", "T3"}},
                    {new Namespace("N3"), new TypeNameSet{"T4"}},
                });
        }

        [TestMethod]
        public void Combine_EmptyWithEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder();
            var configBuilder2 = new AnalyzerConfigBuilder();

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().BeNull();
            configBuilder1.IssueKind.Should().BeNull();
            configBuilder1.InfoImportance.Should().BeNull();
            configBuilder1.Parser.Should().BeNull();
            configBuilder1.ChildCanDependOnParentImplicitly.Should().BeNull();
            configBuilder1.AllowRules.Should().HaveCount(0);
            configBuilder1.DisallowRules.Should().HaveCount(0);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(0);
            configBuilder1.MaxIssueCount.Should().BeNull();
        }

        [TestMethod]
        public void Combine_NonEmptyWithEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetIssueKind(IssueKind.Error)
                .SetInfoImportance(Importance.High)
                .SetParser(Parsers.NRefactory)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet { "T1" })
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet { "T2" })
                .SetMaxIssueCount(42);

            var configBuilder2 = new AnalyzerConfigBuilder();

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.IssueKind.Should().Be(IssueKind.Error);
            configBuilder1.InfoImportance.Should().Be(Importance.High);
            configBuilder1.Parser.Should().Be(Parsers.NRefactory);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [TestMethod]
        public void Combine_EmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder();

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetIssueKind(IssueKind.Error)
                .SetInfoImportance(Importance.High)
                .SetParser(Parsers.NRefactory)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet {"T1"})
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet {"T2"})
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.IssueKind.Should().Be(IssueKind.Error);
            configBuilder1.InfoImportance.Should().Be(Importance.High);
            configBuilder1.Parser.Should().Be(Parsers.NRefactory);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [TestMethod]
        public void Combine_NonEmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(false)
                .SetIssueKind(IssueKind.Info)
                .SetInfoImportance(Importance.Low)
                .SetParser(Parsers.Roslyn)
                .SetChildCanDependOnParentImplicitly(false)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet { "T1" })
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet { "T2" })
                .SetMaxIssueCount(43);

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetIssueKind(IssueKind.Error)
                .SetInfoImportance(Importance.High)
                .SetParser(Parsers.NRefactory)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N6", "N7"), new TypeNameSet { "T3" })
                .AddDisallowRule(new NamespaceDependencyRule("N8", "N9"))
                .AddVisibleTypesByNamespace(new Namespace("N10"), new TypeNameSet { "T4" })
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.IssueKind.Should().Be(IssueKind.Error);
            configBuilder1.InfoImportance.Should().Be(Importance.High);
            configBuilder1.Parser.Should().Be(Parsers.NRefactory);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(2);
            configBuilder1.DisallowRules.Should().HaveCount(2);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(2);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }
    }
}
