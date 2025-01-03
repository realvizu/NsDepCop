using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.RoslynAnalyzer;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Test.RoslynAnalyzer
{
    public class AnalyzerProviderTests
    {
        private readonly Mock<IDependencyAnalyzerFactory> _dependencyAnalyzerFactoryMock;
        private readonly Mock<IAssemblyDependencyAnalyzerFactory> _assemblyDependencyAnalyzerFactoryMock;
        private readonly Mock<IConfigProviderFactory> _configProviderFactoryMock;
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock;

        public AnalyzerProviderTests()
        {
            _dependencyAnalyzerFactoryMock = new Mock<IDependencyAnalyzerFactory>();
            _assemblyDependencyAnalyzerFactoryMock = new Mock<IAssemblyDependencyAnalyzerFactory>();
            _configProviderFactoryMock = new Mock<IConfigProviderFactory>();
            _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsFactory()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            analyzerProvider.GetDependencyAnalyzer(filePath);

            VerifyDependencyAnalyzerFactoryCall(Times.Once());
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CallsFactoryThenAnalyzerRefresh()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            var analyzerMock = new Mock<IDependencyAnalyzer>();
            SetUpFactoryCall(analyzerMock.Object);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            VerifyDependencyAnalyzerFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Never);

            analyzerProvider.GetDependencyAnalyzer(filePath);
            VerifyDependencyAnalyzerFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Once);
        }

        [Fact]
        public void GetAssemblyDependencyAnalyzer_RetrievedTwice_CallsFactoryThenAnalyzerRefresh()
        {
            const string filePath = "myFilePath";

            var analyzerProvider = CreateAnalyzerProvider();

            var analyzerMock = new Mock<IAssemblyDependencyAnalyzer>();
            SetUpFactoryCall(analyzerMock.Object);

            analyzerProvider.GetAssemblyDependencyAnalyzer(filePath);
            VerifyAssemblyDependencyAnalyzerFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Never);

            analyzerProvider.GetAssemblyDependencyAnalyzer(filePath);
            VerifyAssemblyDependencyAnalyzerFactoryCall(Times.Once());
            analyzerMock.Verify(i => i.RefreshConfig(), Times.Once);
        }

        private void SetUpFactoryCall(IDependencyAnalyzer analyzer)
        {
            _dependencyAnalyzerFactoryMock
                .Setup(i => i.Create(It.IsAny<IUpdateableConfigProvider>(), _typeDependencyEnumeratorMock.Object))
                .Returns(analyzer);
        }

        private void VerifyDependencyAnalyzerFactoryCall(Times times)
        {
            _dependencyAnalyzerFactoryMock
                .Verify(i => i.Create(It.IsAny<IUpdateableConfigProvider>(), _typeDependencyEnumeratorMock.Object), times);
        }

        private void VerifyAssemblyDependencyAnalyzerFactoryCall(Times times)
        {
            _assemblyDependencyAnalyzerFactoryMock.Verify(i => i.Create(It.IsAny<IUpdateableConfigProvider>()), times);
        }

        private void SetUpFactoryCall(IAssemblyDependencyAnalyzer analyzer)
        {
            _assemblyDependencyAnalyzerFactoryMock
                .Setup(i => i.Create(It.IsAny<IUpdateableConfigProvider>()))
                .Returns(analyzer);
        }

        private IAnalyzerProvider CreateAnalyzerProvider()
        {
            return new AnalyzerProvider(
                _dependencyAnalyzerFactoryMock.Object,
                _assemblyDependencyAnalyzerFactoryMock.Object,
                _configProviderFactoryMock.Object,
                _typeDependencyEnumeratorMock.Object
            );
        }
    }
}