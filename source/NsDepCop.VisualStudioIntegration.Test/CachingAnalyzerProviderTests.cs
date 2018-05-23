using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    public class CachingAnalyzerProviderTests
    {
        private readonly Mock<IAnalyzerProvider> _embeddedAnalyzerProvider;
        private readonly MockDateTimeProvider _mockDateTimeProvider;
        private static readonly TimeSpan CacheTimeSpan = TimeSpan.FromSeconds(1);

        public CachingAnalyzerProviderTests()
        {
            _embeddedAnalyzerProvider = new Mock<IAnalyzerProvider>();
            _mockDateTimeProvider = new MockDateTimeProvider();
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsEmbeddedProviderOnce()
        {
            const string filePath = "myFilePath";
 
            var cachingAnalyzerProvider = CreateCachingAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            cachingAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimeNotPassed_ReturnsSameItemTwice()
        {
            const string filePath = "myFilePath";

            var cachingAnalyzerProvider = CreateCachingAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            cachingAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            var result2 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            cachingAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [Fact]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimePassed_ReturnsDifferentItems()
        {
            const string filePath = "myFilePath";

            var cachingAnalyzerProvider = CreateCachingAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            cachingAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _mockDateTimeProvider.UtcNow += TimeSpan.FromSeconds(1);

            var result2 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            cachingAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result2);

            _embeddedAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Exactly(2));
        }

        private IAnalyzerProvider CreateCachingAnalyzerProvider()
        {
            return new CachingAnalyzerProvider(_embeddedAnalyzerProvider.Object, _mockDateTimeProvider, CacheTimeSpan);
        }
    }
}
