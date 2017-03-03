using System.Threading;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class XmlFileConfigProviderTests : FileBasedTestsBase
    {
        [TestMethod]
        public void GetState_ConfigNotFound()
        {
            var path = GetTestFilePath("NonExisting.nsdepcop");
            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.NoConfigFile);
            configProvider.ConfigException.Should().BeNull();
        }

        [TestMethod]
        public void GetState_ConfigEnabled()
        {
            var path = GetTestFilePath("Enabled.nsdepcop");
            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.Enabled);
            configProvider.ConfigException.Should().BeNull();
        }

        [TestMethod]
        public void GetState_ConfigDisabled()
        {
            var path = GetTestFilePath("Disabled.nsdepcop");
            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.Disabled);
            configProvider.ConfigException.Should().BeNull();
        }

        [TestMethod]
        public void GetState_ConfigError()
        {
            var path = GetTestFilePath("Erronous.nsdepcop");
            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.ConfigError);
            configProvider.ConfigException.Should().NotBeNull();
        }

        [TestMethod]
        public void GetParser_OverridingParser()
        {
            var path = GetTestFilePath("RoslynParser.nsdepcop");
            {
                var configProvider = new XmlFileConfigProvider(path);
                configProvider.Config.Parser.Should().Be(Parsers.Roslyn);
            }
            {
                var configProvider = new XmlFileConfigProvider(path, Parsers.NRefactory);
                configProvider.Config.Parser.Should().Be(Parsers.NRefactory);
            }
        }

        [TestMethod]
        public void RefreshConfig_EnabledToDisabled()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToDisabled.nsdepcop");

            SetIsEnabled(path, "true");

            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.Enabled);

            Thread.Sleep(10);
            SetIsEnabled(path, "false");

            configProvider.RefreshConfig();
            configProvider.State.Should().Be(AnalyzerState.Disabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToConfigError()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToConfigError.nsdepcop");

            SetIsEnabled(path, "true");

            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.Enabled);

            Thread.Sleep(10);
            SetIsEnabled(path, "maybe");

            configProvider.RefreshConfig();
            configProvider.State.Should().Be(AnalyzerState.ConfigError);
        }

        [TestMethod]
        public void RefreshConfig_NoConfigToEnabled()
        {
            var path = GetTestFilePath("RefreshConfig_NoConfigToEnabled.nsdepcop");

            Delete(path);

            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.NoConfigFile);

            CreateConfigFile(path, "true");

            configProvider.RefreshConfig();
            configProvider.State.Should().Be(AnalyzerState.Enabled);
        }

        [TestMethod]
        public void RefreshConfig_EnabledToNoConfig()
        {
            var path = GetTestFilePath("RefreshConfig_EnabledToNoConfig.nsdepcop");

            Delete(path);
            CreateConfigFile(path, "true");

            var configProvider = new XmlFileConfigProvider(path);
            configProvider.State.Should().Be(AnalyzerState.Enabled);

            Delete(path);

            configProvider.RefreshConfig();
            configProvider.State.Should().Be(AnalyzerState.NoConfigFile);
        }

        private static void CreateConfigFile(string path, string isEnabledString)
        {
            var document = XDocument.Parse($"<NsDepCopConfig IsEnabled='{isEnabledString}'/>");
            document.Save(path);
        }

        private static void SetIsEnabled(string path, string isEnabledString)
        {
            var document = XDocument.Load(path);
            document.Root.Attribute("IsEnabled").SetValue(isEnabledString);
            document.Save(path);
        }
    }
}

