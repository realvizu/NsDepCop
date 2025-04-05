using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Theory]
        [InlineData("/root", "*.cs", "/root/*.cs")]
        [InlineData(@"C:\folder with space", "*.cs", "C:/folder with space/*.cs")]
        [InlineData(@"C:\folder with space", @"**\*.cs", "C:/folder with space/**/*.cs")]
        [InlineData(@"C:\folder with space", @"D:\*.cs", "D:/*.cs")]
        [InlineData(@"C:\folder with space", "//a folder/b.cs", "//a folder/b.cs")]
        [InlineData(@"C:\folder with space", "/*.cs", "/*.cs")]
        public void ToAnalyzerConfig_ConvertsPathsToRooted(string pathRoot, string pathExclusionPattern, string expectedFullPath)
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddSourcePathExclusionPatterns([pathExclusionPattern])
                .MakePathsRooted(pathRoot);

            var config = configBuilder.ToAnalyzerConfig();

            // Note that Path.GetFullPath normalizes the path according to the current OS so we can assert equivalence with the expected path.
            config.SourcePathExclusionPatterns.Select(Path.GetFullPath)
                .Should().BeEquivalentTo(Path.GetFullPath(expectedFullPath));
        }

        [Fact]
        public void SetMethods_WithNonNullValues_OverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetInheritanceDepth(9)
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .SetCheckAssemblyDependencies(true)
                .SetMaxIssueCount(42);

            configBuilder.InheritanceDepth.Should().Be(9);
            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.CheckAssemblyDependencies.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void SetMethods_WithNullValues_DoNotOverwriteProperties()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .SetInheritanceDepth(9)
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .SetCheckAssemblyDependencies(true)
                .SetMaxIssueCount(42);

            configBuilder
                .SetInheritanceDepth(null)
                .SetIsEnabled(null)
                .SetChildCanDependOnParentImplicitly(null)
                .SetCheckAssemblyDependencies(null)
                .SetMaxIssueCount(null);

            configBuilder.InheritanceDepth.Should().Be(9);
            configBuilder.IsEnabled.Should().Be(true);
            configBuilder.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder.CheckAssemblyDependencies.Should().Be(true);
            configBuilder.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void AddAllowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddAllowRule(new DependencyRule("N1", "N2"))
                .AddAllowRule(new DependencyRule("N3", "N4"), new TypeNameSet { "T1", "T2" });

            configBuilder
                .AddAllowRule(new DependencyRule("N3", "N4"), new TypeNameSet { "T2", "T3" })
                .AddAllowRule(new DependencyRule("N5", "N6"), new TypeNameSet { "T4" });

            configBuilder.AllowRules.Should().BeEquivalentTo(
                new Dictionary<DependencyRule, TypeNameSet>
                {
                    {new DependencyRule("N1", "N2"), null},
                    {new DependencyRule("N3", "N4"), new TypeNameSet {"T1", "T2", "T3"}},
                    {new DependencyRule("N5", "N6"), new TypeNameSet {"T4"}},
                });
        }

        [Fact]
        public void AddDisallowRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddDisallowRule(new DependencyRule("N1", "N2"))
                .AddDisallowRule(new DependencyRule("N3", "N4"));

            configBuilder
                .AddDisallowRule(new DependencyRule("N3", "N4"))
                .AddDisallowRule(new DependencyRule("N5", "N6"));

            configBuilder.DisallowRules.Should().BeEquivalentTo(
                new HashSet<DependencyRule>
                {
                    new DependencyRule("N1", "N2"),
                    new DependencyRule("N3", "N4"),
                    new DependencyRule("N5", "N6"),
                });
        }

        [Fact]
        public void AddVisibleTypesByNamespace_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddVisibleTypesByNamespace(new Domain("N1"), null)
                .AddVisibleTypesByNamespace(new Domain("N2"), new TypeNameSet { "T1", "T2" });

            configBuilder
                .AddVisibleTypesByNamespace(new Domain("N2"), new TypeNameSet { "T2", "T3" })
                .AddVisibleTypesByNamespace(new Domain("N3"), new TypeNameSet { "T4" });

            configBuilder.VisibleTypesByNamespace.Should().BeEquivalentTo(
                new Dictionary<Domain, TypeNameSet>
                {
                    {new Domain("N1"), null},
                    {new Domain("N2"), new TypeNameSet {"T1", "T2", "T3"}},
                    {new Domain("N3"), new TypeNameSet {"T4"}},
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
            configBuilder1.AllowedAssemblyRules.Should().HaveCount(0);
            configBuilder1.DisallowedAssemblyRules.Should().HaveCount(0);
            configBuilder1.MaxIssueCount.Should().BeNull();
        }

        [Fact]
        public void AddAllowedAssemblyRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddAllowedAssemblyRule(new DependencyRule("N1", "N2"))
                .AddAllowedAssemblyRule(new DependencyRule("N3", "N4"));

            configBuilder
                .AddAllowedAssemblyRule(new DependencyRule("N3", "N4"))
                .AddAllowedAssemblyRule(new DependencyRule("N5", "N6"));

            configBuilder.AllowedAssemblyRules.Should().BeEquivalentTo(
                new HashSet<DependencyRule>()
                {
                    new DependencyRule("N1", "N2"),
                    new DependencyRule("N3", "N4"),
                    new DependencyRule("N5", "N6"),
                }
            );
        }

        [Fact]
        public void AddDisallowedAssemblyRule_Works()
        {
            var configBuilder = new AnalyzerConfigBuilder()
                .AddDisallowedAssemblyRule(new DependencyRule("A1", "A2"))
                .AddDisallowedAssemblyRule(new DependencyRule("A3", "A4"));

            configBuilder
                .AddDisallowedAssemblyRule(new DependencyRule("A3", "A4"))
                .AddDisallowedAssemblyRule(new DependencyRule("A5", "A6"));

            configBuilder.DisallowedAssemblyRules.Should().BeEquivalentTo(
                new HashSet<DependencyRule>()
                {
                    new DependencyRule("A1", "A2"),
                    new DependencyRule("A3", "A4"),
                    new DependencyRule("A5", "A6"),
                }
            );
        }

        [Fact]
        public void Combine_NonEmptyWithEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new DependencyRule("N1", "N2"), new TypeNameSet { "T1" })
                .AddDisallowRule(new DependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Domain("N5"), new TypeNameSet { "T2" })
                .AddAllowedAssemblyRule(new DependencyRule("A1", "A2"))
                .AddAllowedAssemblyRule(new DependencyRule("A3", "A4"))
                .AddDisallowedAssemblyRule(new DependencyRule("A5", "A6"))
                .AddDisallowedAssemblyRule(new DependencyRule("A7", "A8"))
                .SetMaxIssueCount(42);

            var configBuilder2 = new AnalyzerConfigBuilder();

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.AllowedAssemblyRules.Should().HaveCount(2);
            configBuilder1.DisallowedAssemblyRules.Should().HaveCount(2);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void Combine_EmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder();

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new DependencyRule("N1", "N2"), new TypeNameSet { "T1" })
                .AddDisallowRule(new DependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Domain("N5"), new TypeNameSet { "T2" })
                .AddAllowedAssemblyRule(new DependencyRule("A1", "A2"))
                .AddAllowedAssemblyRule(new DependencyRule("A3", "A4"))
                .AddDisallowedAssemblyRule(new DependencyRule("A5", "A6"))
                .AddDisallowedAssemblyRule(new DependencyRule("A7", "A8"))
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(1);
            configBuilder1.DisallowRules.Should().HaveCount(1);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(1);
            configBuilder1.AllowedAssemblyRules.Should().HaveCount(2);
            configBuilder1.DisallowedAssemblyRules.Should().HaveCount(2);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void Combine_NonEmptyWithNonEmpty()
        {
            var configBuilder1 = new AnalyzerConfigBuilder()
                .SetIsEnabled(false)
                .SetChildCanDependOnParentImplicitly(false)
                .AddAllowRule(new DependencyRule("N1", "N2"), new TypeNameSet { "T1" })
                .AddDisallowRule(new DependencyRule("N3", "N4"))
                .AddVisibleTypesByNamespace(new Domain("N5"), new TypeNameSet { "T2" })
                .AddAllowedAssemblyRule(new DependencyRule("A1", "A2"))
                .AddDisallowedAssemblyRule(new DependencyRule("A3", "A4"))
                .SetMaxIssueCount(43);

            var configBuilder2 = new AnalyzerConfigBuilder()
                .SetIsEnabled(true)
                .SetChildCanDependOnParentImplicitly(true)
                .AddAllowRule(new DependencyRule("N6", "N7"), new TypeNameSet { "T3" })
                .AddDisallowRule(new DependencyRule("N8", "N9"))
                .AddVisibleTypesByNamespace(new Domain("N10"), new TypeNameSet { "T4" })
                .AddAllowedAssemblyRule(new DependencyRule("A6", "A7"))
                .AddAllowedAssemblyRule(new DependencyRule("A8", "A9"))
                .AddDisallowedAssemblyRule(new DependencyRule("A9", "A9"))
                .AddDisallowedAssemblyRule(new DependencyRule("A10", "A10"))
                .SetMaxIssueCount(42);

            configBuilder1.Combine(configBuilder2);

            configBuilder1.IsEnabled.Should().Be(true);
            configBuilder1.ChildCanDependOnParentImplicitly.Should().Be(true);
            configBuilder1.AllowRules.Should().HaveCount(2);
            configBuilder1.DisallowRules.Should().HaveCount(2);
            configBuilder1.VisibleTypesByNamespace.Should().HaveCount(2);
            configBuilder1.AllowedAssemblyRules.Should().HaveCount(3);
            configBuilder1.DisallowedAssemblyRules.Should().HaveCount(3);
            configBuilder1.MaxIssueCount.Should().Be(42);
        }
    }
}