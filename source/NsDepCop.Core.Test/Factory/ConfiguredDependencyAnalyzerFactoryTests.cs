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
        public void CreateInProcess_ConfigIsEnabled()
        {
            var configFilePath = GetFilePathInTestClassFolder("");
            var dependencyAnalyzer = CreateFactory().CreateInProcess(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [Fact]
        public void CreateInProcess_ConfigIsEnabledWithDefaultInfoImportance()
        {
            var configFilePath = GetFilePathInTestClassFolder("");
            var dependencyAnalyzer = CreateFactory()
                .SetDefaultInfoImportance(Importance.High)
                .CreateInProcess(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.InfoImportance.Should().Be(Importance.High);
        }

        private static ConfiguredDependencyAnalyzerFactory CreateFactory() 
            => new ConfiguredDependencyAnalyzerFactory(traceMessageHandler: null);
    }
}
