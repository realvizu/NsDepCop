using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Factory;
using Codartis.NsDepCop.RoslynAnalyzer;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Test.RoslynAnalyzer
{
    public class AnalyzerProviderTests
    {
        private readonly Mock<IDependencyAnalyzerFactory> _dependencyAnalyzerFactoryMock;
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock;

        public AnalyzerProviderTests()
        {
            _dependencyAnalyzerFactoryMock = new Mock<IDependencyAnalyzerFactory>();
            _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsFactory()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            analyzerProvider.GetDependencyAnalyzer(filePath);

            VerifyFactoryCall(Times.Once());
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CallsFactoryThenAnalyzerRefresh()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            var analyzerMock = new Mock<IDependencyAnalyzer>();
            SetUpFactoryCall(analyzerMock.Object);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            VerifyFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Never);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            VerifyFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Once);
        }

        private void SetUpFactoryCall(IDependencyAnalyzer analyzer)
        {
            _dependencyAnalyzerFactoryMock
                .Setup(i => i.Create(It.IsAny<string>(), _typeDependencyEnumeratorMock.Object))
                .Returns(analyzer);
        }

        private void VerifyFactoryCall(Times times)
        {
            _dependencyAnalyzerFactoryMock
                .Verify(i => i.Create(It.IsAny<string>(), _typeDependencyEnumeratorMock.Object), times);
        }

        private IAnalyzerProvider CreateAnalyzerProvider()
        {
            return new AnalyzerProvider(_dependencyAnalyzerFactoryMock.Object, _typeDependencyEnumeratorMock.Object);
        }
    }
}
