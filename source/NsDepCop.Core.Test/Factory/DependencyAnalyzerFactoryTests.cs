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
            var dependencyAnalyzer = CreateDependencyAnalyzerFactory().CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [TestMethod]
        public void CreateFromXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetTestFilePath("config.nsdepcop");
            var dependencyAnalyzer = CreateDependencyAnalyzerFactory().SetDefaultInfoImportance(Importance.High).CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_Enabled()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = CreateDependencyAnalyzerFactory().CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = CreateDependencyAnalyzerFactory().SetDefaultInfoImportance(Importance.High).CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        private static DependencyAnalyzerFactory CreateDependencyAnalyzerFactory()
        {
            return new DependencyAnalyzerFactory();
        }

    }
}
