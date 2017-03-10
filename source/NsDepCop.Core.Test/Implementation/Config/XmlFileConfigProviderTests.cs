using System;
using System.Threading;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class XmlFileConfigProviderTests : XmlFileConfigTestBase
    {
        [TestMethod]
        public void Properties_ConfigNotFound()
        {
            var path = GetTestFilePath("NonExisting.nsdepcop");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigEnabled()
        {
            var path = GetTestFilePath("Enabled.nsdepcop");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().NotBeNull();
        }

        [TestMethod]
        public void Properties_ConfigDisabled()
        {
            var path = GetTestFilePath("Disabled.nsdepcop");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
            configProvider.ConfigException.Should().BeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void Properties_ConfigError()
        {
            var path = GetTestFilePath("Erronous.nsdepcop");
            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
            configProvider.ConfigException.Should().NotBeNull();
            configProvider.Config.Should().BeNull();
        }

        [TestMethod]
        public void OverridingParser()
        {
            var path = GetTestFilePath("RoslynParser.nsdepcop");
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
        public void RefreshConfig_Unchanged()
        {
            var path = GetTestFilePath("Enabled.nsdepcop");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            var savedConfig = configProvider.Config;

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            configProvider.Config.Should().Be(savedConfig);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToDisabled()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToDisabled.nsdepcop");

            SetIsEnabled(path, "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetIsEnabled(path, "false");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Disabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToConfigError()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToConfigError.nsdepcop");

            SetIsEnabled(path, "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Thread.Sleep(10);
            SetIsEnabled(path, "maybe");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.ConfigError);
        }

        [TestMethod]
        public void RefreshConfig_NoConfigToEnabled()
        {
            var path = GetTestFilePath("RefreshConfig_NoConfigToEnabled.nsdepcop");

            Delete(path);

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);

            CreateConfigFile(path, "true");

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToNoConfig()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToNoConfig.nsdepcop");

            Delete(path);
            CreateConfigFile(path, "true");

            var configProvider = CreateConfigProvider(path);
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.Enabled);

            Delete(path);

            configProvider.RefreshConfig();
            configProvider.ConfigState.Should().Be(AnalyzerConfigState.NoConfig);
        }

        private static XmlFileConfigProvider CreateConfigProvider(string path, Parsers? overridingParser = null) 
            => new XmlFileConfigProvider(path, overridingParser, Console.WriteLine);
    }
}

