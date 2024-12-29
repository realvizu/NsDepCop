using System.Collections.Generic;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Config.Implementation;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Config
{
    public class AnalyzerConfigBuilderTests
    {
        [Fact]
        public void ToAnalyzerConfig_AppliesDefaults()
        {
            var config = new AnalyzerConfigBuilder().ToAnalyzerConfig();

            config.IsEnabled.Should().Be(ConfigDefaults.IsEnabled);
            config.ChildCanDependOnParentImplicitly.Should().Be(ConfigDefaults.ChildCanDependOnParentImplicitly);
            config.MaxIssueCount.Should().Be(ConfigDefaults.MaxIssueCount);
        }

        [Fact]
        public void ToAnalyzerConfig_ConvertsPathsToRooted()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddSourcePathExclusionPatterns(new[]
                {
                    @"*.cs",
                    @"**\*.cs",
                    @"D:\*.cs",
                    @"\\a folder\b.cs",
                    @"\*.cs"
                })
                .MakePathsRooted(@"C:\folder with space");

            var config = configBuilder.ToAnalyzerConfig();

            config.SourcePathExclusionPatterns.Should().BeEquivalentTo(
                @"C:\folder with space\*.cs",
                @"C:\folder with space\**\*.cs",
                @"D:\*.cs",
                @"\\a folder\b.cs",
                @"\*.cs"
            );
        }

        [Fact]
        public void SetMethods_WithNonNullValues_OverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetInheritanceDepth(9)
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .SetMaxIssueCount(42);

            configBuilder.InheritanceDepth.Should().Be(9);
            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void SetMethods_WithNullValues_DoNotOverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetInheritanceDepth(9)
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .SetMaxIssueCount(42);

            configBuilder
                .SetInheritanceDepth(null)
                .SetIsEnabled(null)
                .SetChildCanDependOnParentImplicitly(null)
                .SetMaxIssueCount(null);

            configBuilder.InheritanceDepth.Should().Be(9);
            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void AddAllowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"))
                .AddAllowRule(new NamespaceDependencyRule("N3", "N4"), new TypeNameSet {"T1", "T2"});

            configBuilder
                .AddAllowRule(new NamespaceDependencyRule("N3", "N4"), new TypeNameSet {"T2", "T3"})
                .AddAllowRule(new NamespaceDependencyRule("N5", "N6"), new TypeNameSet {"T4"});

            configBuilder.AllowRules.Should().BeEquivalentTo(
                new Dictionary<NamespaceDependencyRule, TypeNameSet>
                {
                    {new NamespaceDependencyRule("N1", "N2"), null},
                    {new NamespaceDependencyRule("N3", "N4"), new TypeNameSet {"T1", "T2", "T3"}},
                    {new NamespaceDependencyRule("N5", "N6"), new TypeNameSet {"T4"}},
                });
        }

        [Fact]
        public void AddDisallowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddDisallowRule(new NamespaceDependencyRule("N1", "N2"))
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"));

            configBuilder
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddDisallowRule(new NamespaceDependencyRule("N5", "N6"));

            configBuilder.DisallowRules.Should().BeEquivalentTo(
                new HashSet<NamespaceDependencyRule>
                {
                    new NamespaceDependencyRule("N1", "N2"),
                    new NamespaceDependencyRule("N3", "N4"),
                    new NamespaceDependencyRule("N5", "N6"),
                });
        }

        [Fact]
        public void AddVisibleTypesByNamespace_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddVisibleTypesByNamespace(new Namespace("N1"), null)
                .AddVisibleTypesByNamespace(new Namespace("N2"), new TypeNameSet {"T1", "T2"});

            configBuilder
                .AddVisibleTypesByNamespace(new Namespace("N2"), new TypeNameSet {"T2", "T3"})
                .AddVisibleTypesByNamespace(new Namespace("N3"), new TypeNameSet {"T4"});

            configBuilder.VisibleTypesByNamespace.Should().BeEquivalentTo(
                new Dictionary<Namespace, TypeNameSet>
                {
                    {new Namespace("N1"), null},
                    {new Namespace("N2"), new TypeNameSet {"T1", "T2", "T3"}},
                    {new Namespace("N3"), new TypeNameSet {"T4"}},
                });
        }

        [Fact]
        public void Combine_EmptyWithEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder();
            var configBuilder2 = new AnalyzerConfigBuilder();

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().BeNull();
            configBuilder1.ChildCanDependOnParentImplicitly.Should().BeNull();
            configBuilder1.AllowRules.Should().HaveCount(0);
            configBuilder1.DisallowRules.Should().HaveCount(0);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(0);
            configBuilder1.MaxIssueCount.Should().BeNull();
        }

        [Fact]
        public void Combine_NonEmptyWithEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet {"T1"})
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet {"T2"})
                .SetMaxIssueCount(42);

            var configBuilder2 = new AnalyzerConfigBuilder();

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void Combine_EmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder();

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet {"T1"})
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet {"T2"})
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void Combine_NonEmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(false)
                .SetChildCanDependOnParentImplicitly(false)
                .AddAllowRule(new NamespaceDependencyRule("N1", "N2"), new TypeNameSet {"T1"})
                .AddDisallowRule(new NamespaceDependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Namespace("N5"), new TypeNameSet {"T2"})
                .SetMaxIssueCount(43);

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new NamespaceDependencyRule("N6", "N7"), new TypeNameSet {"T3"})
                .AddDisallowRule(new NamespaceDependencyRule("N8", "N9"))
                .AddVisibleTypesByNamespace(new Namespace("N10"), new TypeNameSet {"T4"})
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(2);
            configBuilder1.DisallowRules.Should().HaveCount(2);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(2);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }
    }
}