using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    public class AnalyzerProviderTests
    {
        private readonly Mock<IConfiguredDependencyAnalyzerFactory> _dependencyAnalyzerFactoryMock;

        public AnalyzerProviderTests()
        {
            _dependencyAnalyzerFactoryMock = new Mock<IConfiguredDependencyAnalyzerFactory>();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsFactory()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            analyzerProvider.GetDependencyAnalyzer(filePath);

            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CallsFactoryThenAnalyzerRefresh()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            var analyzerMock = new Mock<IConfiguredDependencyAnalyzer>();
            _dependencyAnalyzerFactoryMock.Setup(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()))
                .Returns(analyzerMock.Object);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Never);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            _dependencyAnalyzerFactoryMock.Verify(i => i.CreateFromMultiLevelXmlConfigFile(It.IsAny<string>()), Times.Once);
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Once);
        }

        private IAnalyzerProvider CreateAnalyzerProvider()
        {
            return new AnalyzerProvider(_dependencyAnalyzerFactoryMock.Object);
        }
    }
}
