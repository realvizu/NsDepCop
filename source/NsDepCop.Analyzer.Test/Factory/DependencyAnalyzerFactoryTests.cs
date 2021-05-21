using Codartis.NsDepCop.Factory;
using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Interface.Config;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Analyzer.Test.Factory
{
    public class DependencyAnalyzerFactoryTests : FileBasedTestsBase
    {
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Fact]
        public void CreateInProcess_DefaultInfoImportance()
        {
            var configFilePath = GetFilePathInTestClassFolder("");

            var dependencyAnalyzer = CreateFactory()
                .Create(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.InfoImportance.Should().Be(ConfigDefaults.InfoImportance);
        }

        [Fact]
        public void CreateInProcess_InfoImportanceAppliedToFactory()
        {
            var configFilePath = GetFilePathInTestClassFolder("");

            var dependencyAnalyzer = CreateFactory()
                .SetDefaultInfoImportance(Importance.High)
                .Create(configFilePath, _typeDependencyEnumeratorMock.Object);

            dependencyAnalyzer.InfoImportance.Should().Be(Importance.High);
        }

        private static DependencyAnalyzerFactory CreateFactory() 
            => new DependencyAnalyzerFactory(traceMessageHandler: null);
    }
}
