using System;
using System.IO;
using System.Threading;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class MultiLevelXmlFileConfigProviderTests : XmlFileConfigTestBase
    {
        [TestMethod]
        public void Rules_Merged()
        {
            var folder = GetTestFilePath(@"Rules_Merged\Level2\Level1");
            var configProvider = CreateConfigProvider(folder);

            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();

            var allowedRules = configProvider.Config.AllowRules;
            allowedRules.Should().HaveCount(2);
            allowedRules.Keys.Should().Contain(new NamespaceDependencyRule("N1", "N2"));
            allowedRules.Keys.Should().Contain(new NamespaceDependencyRule("N3", "N4"));
        }

        [TestMethod]
        public void Attributes_LowerLevelWins()
        {
            var folder = GetTestFilePath(@"Attributes_LowerLevelWins\Level2\Level1");
            var configProvider = CreateConfigProvider(folder);
            configProvider.Config.IssueKind.Should().Be(IssueKind.Info);
        }

        [TestMethod]
        public void Attributes_MissingDoesNotOverwrite()
        {
            var folder = GetTestFilePath(@"Attributes_MissingDoesNotOverwrite\Level2\Level1");
            var configProvider = CreateConfigProvider(folder);
            configProvider.Config.IssueKind.Should().Be(IssueKind.Error);
        }

        [TestMethod]
        public void Properties_ConfigNotFound()
        {
            var path = GetTestFilePath(@"NoConfig\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigError()
        {
            var path = GetTestFilePath(@"ConfigError\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
            configProvider.ConfigException.Should().NotBeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigEnabled()
        {
            var path = GetTestFilePath(@"ConfigEnabled\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Properties_ConfigDisabledAtProjectLevel_EffectiveDisabled()
        {
            var path = GetTestFilePath(@"ConfigDisabledAtProjectLevel\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigDisabledAtHigherLevelAndUndefinedAtProjectLevel_EffectiveDisabled()
        {
            var path = GetTestFilePath(@"ConfigDisabledAtHigherLevelAndUndefinedAtProjectLevel\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigDisabledAtHigherLevelButEnabledAtProjectLevel_DisabledConfigNotCombinedToEffective()
        {
            var path = GetTestFilePath(@"ConfigDisabledAtHigherLevelButEnabledAtProjectLevel\Level2\Level1");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.IssueKind.Should().Be(ConfigDefaults.IssueKind);
        }

        [TestMethod]
        public void OverridingParser()
        {
            var path = GetTestFilePath("RoslynParser");
            {
                var configProvider = CreateConfigProvider(path);
                configProvider.Config.Parser.Should().Be(Parsers.Roslyn);
            }
            {
                var configProvider = CreateConfigProvider(path, Parsers.NRefactory);
                configProvider.Config.Parser.Should().Be(Parsers.NRefactory);
            }
        }

        [TestMethod]
        public void DefaultParser()
        {
            {
                var path = GetTestFilePath("RoslynParser");
                var configProvider = CreateConfigProvider(path).SetDefaultParser(Parsers.NRefactory);
                configProvider.Config.Parser.Should().Be(Parsers.Roslyn);
            }
            {
                var path = GetTestFilePath("NoAttributes");
                var configProvider = CreateConfigProvider(path).SetDefaultParser(Parsers.NRefactory);
                configProvider.Config.Parser.Should().Be(Parsers.NRefactory);
            }
        }

        [TestMethod]
        public void DefaultInfoImportance()
        {
            {
                var path = GetTestFilePath("HighInfoImportance");
                var configProvider = CreateConfigProvider(path).SetDefaultInfoImportance(Importance.Low);
                configProvider.Config.InfoImportance.Should().Be(Importance.High);
            }
            {
                var path = GetTestFilePath("NoAttributes");
                var configProvider = CreateConfigProvider(path).SetDefaultInfoImportance(Importance.Low); 
                configProvider.Config.InfoImportance.Should().Be(Importance.Low);
            }
        }

        [TestMethod]
        public void RefreshConfig_Unchanged()
        {
            var path = GetTestFilePath(@"ConfigEnabled\Level2\Level1");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            var savedConfig = configProvider.Config;

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.Config.Should().Be(savedConfig);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToEnabledButChanged()
        {
            var path = GetTestFilePath(@"RefreshConfig_EnabledToEnabledButChanged\Level2\Level1");
            var path2 = GetTestFilePath(@"RefreshConfig_EnabledToEnabledButChanged");

            SetAttribute(GetConfigFilePath(path), "CodeIssueKind", "Error");
            SetAttribute(GetConfigFilePath(path2), "MaxIssueCount", "1");

            var configProvider = CreateConfigProvider(path);
            configProvider.Config.IssueKind.Should().Be(IssueKind.Error);
            configProvider.Config.MaxIssueCount.Should().Be(1);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "CodeIssueKind", "Info");
            SetAttribute(GetConfigFilePath(path2), "MaxIssueCount", "2");

            configProvider.RefreshConfig();
            configProvider.Config.IssueKind.Should().Be(IssueKind.Info);
            configProvider.Config.MaxIssueCount.Should().Be(2);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToDisabled()
        {
            var path = GetTestFilePath(@"RefreshConfig_EnabledToDisabled\Level2\Level1");

            SetAttribute(GetConfigFilePath(path), "IsEnabled", "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "IsEnabled", "false");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToConfigError()
        {
            var path = GetTestFilePath(@"RefreshConfig_EnabledToConfigError\Level2\Level1");

            SetAttribute(GetConfigFilePath(path), "IsEnabled", "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetAttribute(GetConfigFilePath(path), "IsEnabled", "maybe");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
        }

        [TestMethod]
        public void RefreshConfig_NoConfigToEnabled()
        {
            var path = GetTestFilePath(@"RefreshConfig_NoConfigToEnabled\Level2\Level1");

            Delete(GetConfigFilePath(path));

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);

            CreateConfigFile(GetConfigFilePath(path), "true", 2);

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToNoConfig()
        {
            var path = GetTestFilePath(@"RefreshConfig_EnabledToNoConfig\Level2\Level1");

            Delete(path);
            CreateConfigFile(GetConfigFilePath(path), "true", 2);

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Delete(GetConfigFilePath(path));

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
        }

        [TestMethod]
        public void RefreshConfig_InheritanceDepthChangedFrom0To2()
        {
            var path = GetTestFilePath(@"RefreshConfig_InheritanceDepthChanged\Level2\Level1");

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

        [TestMethod]
        public void RefreshConfig_InheritanceDepthChangedFrom2To0()
        {
            var path = GetTestFilePath(@"RefreshConfig_InheritanceDepthChanged\Level2\Level1");

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

        private static string GetConfigFilePath(string path)
        {
            return Path.Combine(path, ProductConstants.DefaultConfigFileName);
        }

        private static MultiLevelXmlFileConfigProvider CreateConfigProvider(string folder, Parsers? overridingParser = null)
        {
            return new MultiLevelXmlFileConfigProvider(folder, Console.WriteLine)
                .OverrideParser(overridingParser) as MultiLevelXmlFileConfigProvider;
        }
    }
}
