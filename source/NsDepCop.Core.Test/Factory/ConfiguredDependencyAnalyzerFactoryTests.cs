using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Factory
{
    public class ConfiguredDependencyAnalyzerFactoryTests : FileBasedTestsBase
    {
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Fact]
        public void CreateFromXmlConfigFile_Enabled()
        {
            var configFilePath = GetFilePathInTestClassFolder("config.nsdepcop");
            var dependencyAnalyzer = CreateFactory().CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [Fact]
        public void CreateFromXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetFilePathInTestClassFolder("config.nsdepcop");
            var dependencyAnalyzer = CreateFactory().SetDefaultInfoImportance(Importance.High).CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        [Fact]
        public void CreateFromMultiLevelXmlConfigFile_Enabled()
        {
            var configFilePath = GetFilePathInTestClassFolder("");
            var dependencyAnalyzer = CreateFactory().CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [Fact]
        public void CreateFromMultiLevelXmlConfigFile_EnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetFilePathInTestClassFolder("");
            var dependencyAnalyzer = CreateFactory().SetDefaultInfoImportance(Importance.High).CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        private  ConfiguredDependencyAnalyzerFactory CreateFactory() 
            => new ConfiguredDependencyAnalyzerFactory(_typeDependencyEnumeratorMock.Object, traceMessageHandler: null);
    }
}
