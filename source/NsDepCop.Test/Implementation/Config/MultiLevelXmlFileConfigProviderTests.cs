﻿using System.IO;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Config.Implementation;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Config
{
    public class MultiLevelXmlFileConfigProviderTests : XmlFileConfigTestBase
    {
        [Fact]
        public void Rules_Merged()
        {
            var folder = GetFilePathInTestClassFolder(@"Rules_Merged/Level2/Level1");
            var configProvider = CreateConfigProvider(folder);

            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();

            var allowedRules = configProvider.Config.AllowRules;
            allowedRules.Should().HaveCount(2);
            allowedRules.Keys.Should().Contain(new DependencyRule("N1", "N2"));
            allowedRules.Keys.Should().Contain(new DependencyRule("N3", "N4"));
        }

        [Fact]
        public void Attributes_LowerLevelWins()
        {
            var folder = GetFilePathInTestClassFolder("Attributes_LowerLevelWins/Level2/Level1");
            var configProvider = CreateConfigProvider(folder);
            configProvider.Config.MaxIssueCount.Should().Be(2);
        }

        [Fact]
        public void Attributes_MissingDoesNotOverwrite()
        {
            var folder = GetFilePathInTestClassFolder("Attributes_MissingDoesNotOverwrite/Level2/Level1");
            var configProvider = CreateConfigProvider(folder);
            configProvider.Config.MaxIssueCount.Should().Be(1);
        }

        [Fact]
        public void ExcludedFiles_AllCorrectlyRooted()
        {
            var folder = GetFilePathInTestClassFolder("ExcludedFiles_AllCorrectlyRooted/Level2/Level1");

            var expectedExcludedFiles = new[]
            {
                GetFilePathInTestClassFolder("ExcludedFiles_AllCorrectlyRooted/Level2/Level1/ExcludedFile1.cs"),
                GetFilePathInTestClassFolder("ExcludedFiles_AllCorrectlyRooted/Level2/Level1/Excluded File 2.cs"),
                GetFilePathInTestClassFolder("ExcludedFiles_AllCorrectlyRooted/Level2/ExcludedFile3.cs"),
                GetFilePathInTestClassFolder("ExcludedFiles_AllCorrectlyRooted/Level2/Excluded File 4.cs"),
            };

            var configProvider = CreateConfigProvider(folder);
            configProvider.Config.SourcePathExclusionPatterns.Select(Path.GetFullPath).Should().BeEquivalentTo(expectedExcludedFiles.Select(Path.GetFullPath));
            configProvider.Config.SourcePathExclusionPatterns.All(File.Exists).Should().BeTrue();
        }

        [Fact]
        public void Properties_ConfigNotFound()
        {
            var path = GetFilePathInTestClassFolder("NoConfig/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [Fact]
        public void Properties_ConfigError()
        {
            var path = GetFilePathInTestClassFolder("ConfigError/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
            configProvider.ConfigException.Should().NotBeNull();
            configProvider.Config.Should().BeNull();
        }

        [Fact]
        public void Properties_ConfigEnabled()
        {
            var path = GetFilePathInTestClassFolder("ConfigEnabled/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().NotBeNull();
        }

        [Fact]
        public void Properties_ConfigDisabledAtProjectLevel_EffectiveDisabled()
        {
            var path = GetFilePathInTestClassFolder("ConfigDisabledAtProjectLevel/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [Fact]
        public void Properties_ConfigDisabledAtHigherLevelAndUndefinedAtProjectLevel_EffectiveDisabled()
        {
            var path = GetFilePathInTestClassFolder("ConfigDisabledAtHigherLevelAndUndefinedAtProjectLevel/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [Fact]
        public void Properties_ConfigDisabledAtHigherLevelButEnabledAtProjectLevel_DisabledConfigNotCombinedToEffective()
        {
            var path = GetFilePathInTestClassFolder("ConfigDisabledAtHigherLevelButEnabledAtProjectLevel/Level2/Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.MaxIssueCount.Should().Be(ConfigDefaults.MaxIssueCount);
        }

        [Fact]
        public void RefreshConfig_Unchanged()
        {
            var path = GetFilePathInTestClassFolder("ConfigEnabled/Level2/Level1");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            var savedConfig = configProvider.Config;

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.Config.Should().Be(savedConfig);
        }

        [Fact]
        public void RefreshConfig_EnabledToEnabledButChanged()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_EnabledToEnabledButChanged/Level2/Level1");
            var path2 = GetFilePathInTestClassFolder("RefreshConfig_EnabledToEnabledButChanged");

            SetAttribute(GetConfigFilePath(path), "AutoLowerMaxIssueCount", "true");
            SetAttribute(GetConfigFilePath(path2), "MaxIssueCount", "1");

            var configProvider = CreateConfigProvider(path);
            configProvider.Config.AutoLowerMaxIssueCount.Should().Be(true);
            configProvider.Config.MaxIssueCount.Should().Be(1);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "AutoLowerMaxIssueCount", "false");
            SetAttribute(GetConfigFilePath(path2), "MaxIssueCount", "2");

            configProvider.RefreshConfig();
            configProvider.Config.AutoLowerMaxIssueCount.Should().Be(false);
            configProvider.Config.MaxIssueCount.Should().Be(2);
        }

        [Fact]
        public void RefreshConfig_EnabledToDisabled()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_EnabledToDisabled/Level2/Level1");

            SetAttribute(GetConfigFilePath(path), "IsEnabled", "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "IsEnabled", "false");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
        }

        [Fact]
        public void RefreshConfig_EnabledToConfigError()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_EnabledToConfigError/Level2/Level1");

            SetAttribute(GetConfigFilePath(path), "IsEnabled", "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "IsEnabled", "maybe");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
        }

        [Fact]
        public void RefreshConfig_NoConfigToEnabled()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_NoConfigToEnabled/Level2/Level1");

            Delete(GetConfigFilePath(path));

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);

            CreateConfigFile(GetConfigFilePath(path), "true", 2);

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
        }

        [Fact]
        public void RefreshConfig_EnabledToNoConfig()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_EnabledToNoConfig/Level2/Level1");

            Delete(path);
            CreateConfigFile(GetConfigFilePath(path), "true", 2);

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Delete(GetConfigFilePath(path));

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
        }

        [Fact]
        public void RefreshConfig_InheritanceDepthChangedFrom0To2()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_InheritanceDepthChanged/Level2/Level1");

            SetAttribute(GetConfigFilePath(path), "InheritanceDepth", "0");

            var configProvider = CreateConfigProvider(path);
            configProvider.InheritanceDepth.Should().Be(0);
            configProvider.Config.MaxIssueCount.Should().Be(ConfigDefaults.MaxIssueCount);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "InheritanceDepth", "2");

            configProvider.RefreshConfig();
            configProvider.InheritanceDepth.Should().Be(2);
            configProvider.Config.MaxIssueCount.Should().Be(42);
        }

        [Fact]
        public void RefreshConfig_InheritanceDepthChangedFrom2To0()
        {
            var path = GetFilePathInTestClassFolder("RefreshConfig_InheritanceDepthChanged/Level2/Level1");

            SetAttribute(GetConfigFilePath(path), "InheritanceDepth", "2");

            var configProvider = CreateConfigProvider(path);
            configProvider.InheritanceDepth.Should().Be(2);
            configProvider.Config.MaxIssueCount.Should().Be(42);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "InheritanceDepth", "0");

            configProvider.RefreshConfig();
            configProvider.InheritanceDepth.Should().Be(0);
            configProvider.Config.MaxIssueCount.Should().Be(ConfigDefaults.MaxIssueCount);
        }

        [Fact]
        public void UpdateMaxIssueCount_Level1ContainsNoMaxIssueCount_SetToNewValue()
        {
            var path = GetFilePathInTestClassFolder("UpdateMaxIssueCount_Level1ContainsNoMaxIssueCount/Level2/Level1");

            RemoveAttribute(GetConfigFilePath(path), "MaxIssueCount");

            var configProvider = CreateConfigProvider(path);
            configProvider.Config.MaxIssueCount.Should().Be(42);

            Thread.Sleep(10);
            configProvider.UpdateMaxIssueCount(142);
            configProvider.Config.MaxIssueCount.Should().Be(142);

            GetAttribute(GetConfigFilePath(path.Replace("/Level1", "")), "MaxIssueCount").Should().Be(42.ToString());
            GetAttribute(GetConfigFilePath(path), "MaxIssueCount").Should().Be(142.ToString());
        }

        [Fact]
        public void UpdateMaxIssueCount_Level1ContainsMaxIssueCount_SetToNewValue()
        {
            var path = GetFilePathInTestClassFolder("UpdateMaxIssueCount_Level1ContainsMaxIssueCount/Level2/Level1");

            SetAttribute(GetConfigFilePath(path), "MaxIssueCount", 42.ToString());

            var configProvider = CreateConfigProvider(path);
            configProvider.Config.MaxIssueCount.Should().Be(42);

            Thread.Sleep(10);
            configProvider.UpdateMaxIssueCount(142);
            configProvider.Config.MaxIssueCount.Should().Be(142);

            GetAttribute(GetConfigFilePath(path.Replace("/Level1", "")), "MaxIssueCount").Should().BeNull();
            GetAttribute(GetConfigFilePath(path), "MaxIssueCount").Should().Be(142.ToString());
        }

        private static string GetConfigFilePath(string path)
        {
            return Path.Combine(path, ProductConstants.DefaultConfigFileName);
        }

        private static MultiLevelXmlFileConfigProvider CreateConfigProvider(string folder)
        {
            return new MultiLevelXmlFileConfigProvider(folder, traceMessageHandler: null);
        }
    }
}
