using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Factory
{
    [TestClass]
    public class DependencyAnalyzerFactoryTests : FileBasedTestsBase
    {
        [TestMethod]
        public void CreateFromXmlConfigFile_Enabled()
        {
            var configFilePath = GetTestFilePath("config.nsdepcop");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [TestMethod]
        public void CreateFromXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetTestFilePath("config.nsdepcop");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().SetDefaultInfoImportance(Importance.High).CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_Enabled()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().SetDefaultInfoImportance(Importance.High).CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }
    }
}
