using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    public class CachingDependencyAnalyzerProviderTests
    {
        private readonly Mock<IAnalyzerProvider> _embeddedDependencyAnalyzerProvider;
        private readonly MockDateTimeProvider _mockDateTimeProvider;
        private static readonly TimeSpan CacheTimeSpan = TimeSpan.FromSeconds(1);

        public CachingDependencyAnalyzerProviderTests()
        {
            _embeddedDependencyAnalyzerProvider = new Mock<IAnalyzerProvider>();
            _mockDateTimeProvider = new MockDateTimeProvider();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsEmbeddedProviderOnce()
        {
            const string filePath = "myFilePath";
 
            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IRefreshableDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimeNotPassed_ReturnsSameItemTwice()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IRefreshableDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            var result2 = new Mock<IRefreshableDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimePassed_ReturnsDifferentItems()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IRefreshableDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _mockDateTimeProvider.UtcNow += TimeSpan.FromSeconds(1);

            var result2 = new Mock<IRefreshableDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result2);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Exactly(2));
        }

        private IAnalyzerProvider CreateDependencyAnalyzerProvider()
        {
            return new CachingAnalyzerProvider(_embeddedDependencyAnalyzerProvider.Object, _mockDateTimeProvider, CacheTimeSpan);
        }
    }
}
