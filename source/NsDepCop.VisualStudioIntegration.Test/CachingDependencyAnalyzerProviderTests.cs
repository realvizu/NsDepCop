using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    [TestClass]
    public class CachingDependencyAnalyzerProviderTests
    {
        private Mock<IDependencyAnalyzerProvider> _embeddedDependencyAnalyzerProvider;
        private MockDateTimeProvider _mockDateTimeProvider;
        private static readonly TimeSpan CacheTimeSpan = TimeSpan.FromSeconds(1);

        [TestInitialize]
        public void TestInitialize()
        {
            _embeddedDependencyAnalyzerProvider = new Mock<IDependencyAnalyzerProvider>();
            _mockDateTimeProvider = new MockDateTimeProvider();
        }

        [TestMethod]
        public void GetDependencyAnalyzer_RetrievedOnce_CallsEmbeddedProviderOnce()
        {
            const string filePath = "myFilePath";
 
            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [TestMethod]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimeNotPassed_ReturnsSameItemTwice()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            var result2 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Once);
        }

        [TestMethod]
        public void GetDependencyAnalyzer_RetrievedTwice_CacheTimePassed_ReturnsDifferentItems()
        {
            const string filePath = "myFilePath";

            var dependencyAnalyzerProvider = CreateDependencyAnalyzerProvider();

            var result1 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result1);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result1);

            _mockDateTimeProvider.UtcNow += TimeSpan.FromSeconds(1);

            var result2 = new Mock<IDependencyAnalyzer>().Object;
            _embeddedDependencyAnalyzerProvider.Setup(i => i.GetDependencyAnalyzer(filePath)).Returns(result2);
            dependencyAnalyzerProvider.GetDependencyAnalyzer(filePath).Should().Be(result2);

            _embeddedDependencyAnalyzerProvider.Verify(i => i.GetDependencyAnalyzer(filePath), Times.Exactly(2));
        }

        private IDependencyAnalyzerProvider CreateDependencyAnalyzerProvider()
        {
            return new CachingDependencyAnalyzerProvider(_embeddedDependencyAnalyzerProvider.Object, _mockDateTimeProvider, CacheTimeSpan);
        }
    }
}
