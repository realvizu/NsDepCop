using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    public class DependencyAnalyzerProviderTests
    {
        private readonly Mock<IDependencyAnalyzerFactory> _dependencyAnalyzerFactoryMock;

        public DependencyAnalyzerProviderTests()
        {
            _dependencyAnalyzerFactoryMock = new Mock<IDependencyAnalyzerFactory>();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsFactory()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath);

            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CallsFactoryThenAnalyzerRefresh()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var analyzerMock = new Mock<IRefreshableDependencyAnalyzer>();
            _dependencyAnalyzerFactoryMock.Setup(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()))
                .Returns(analyzerMock.Object);

            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath);
            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Never);

            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath);
            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Once);
        }

        private IAnalyzerProvider CreateDependencyAnalyzerProvider()
        {
            return new AnalyzerProvider(_dependencyAnalyzerFactoryMock.Object);
        }
    }
}
