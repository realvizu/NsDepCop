using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Analysis.Configured;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
{
    public class ConfiguredDependencyAnalyzerTests
    {
        private static readonly SourceSegment DummySourceSegment = new SourceSegment(1, 1, 1, 1, null, null);

        private readonly Mock<IUpdateableConfigProvider> _configProviderMock = new Mock<IUpdateableConfigProvider>();
        private readonly Mock<IAnalyzerConfig> _configMock = new Mock<IAnalyzerConfig>();
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Theory]
        [InlineData(AnalyzerConfigState.NoConfig)]
        [InlineData(AnalyzerConfigState.ConfigError)]
        [InlineData(AnalyzerConfigState.Disabled)]
        public void AnalyzeSyntaxNode_ConfigNotEnabled_Exception(AnalyzerConfigState configState)
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(configState);

            var dependencyAnalyzer = CreateAnalyzer();
            Assert.Throws<InvalidOperationException>(() => dependencyAnalyzer.AnalyzeSyntaxNode(null, null));
        }

        [Theory]
        [InlineData(AnalyzerConfigState.NoConfig)]
        [InlineData(AnalyzerConfigState.ConfigError)]
        [InlineData(AnalyzerConfigState.Disabled)]
        public void AnalyzeProject_ConfigNotEnabled_Exception(AnalyzerConfigState configState)
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(configState);

            var dependencyAnalyzer = CreateAnalyzer();
            Assert.Throws<InvalidOperationException>(() => dependencyAnalyzer.AnalyzeProject(null, null));
        }

        [Fact]
        public void RefreshConfig_Works()
        {
            SetUpEnabledConfig(maxIssueCount: 2);

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 10));

            var dependencyAnalyzer = CreateAnalyzer();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().HaveCount(2);

            SetUpEnabledConfig(maxIssueCount: 4);
            dependencyAnalyzer.RefreshConfig();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().HaveCount(4);
        }

        [Theory]
        [InlineData(3, 2, true)]
        [InlineData(3, 3, false)]
        [InlineData(3, 4, false)]
        public void AutoLowerMaxIssueCount_Works(int maxIssueCount, int actualIssueCount, bool isUpdateCalled)
        {
            SetUpEnabledConfig(maxIssueCount: maxIssueCount, autoLowerMaxIssueCount: true);

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), actualIssueCount));

            var expectedIssueCount = Math.Min(actualIssueCount, maxIssueCount);

            var dependencyAnalyzer = CreateAnalyzer();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().HaveCount(expectedIssueCount);

            if (isUpdateCalled)
                _configProviderMock.Verify(i => i.UpdateMaxIssueCount(expectedIssueCount));
            else
                _configProviderMock.Verify(i => i.UpdateMaxIssueCount(It.IsAny<int>()), Times.Never);
        }

        private void SetUpEnabledConfig(int maxIssueCount = 100, bool autoLowerMaxIssueCount = false)
        {
            _configMock.Setup(i => i.AllowRules).Returns(new Dictionary<NamespaceDependencyRule, TypeNameSet>());
            _configMock.Setup(i => i.DisallowRules).Returns(new HashSet<NamespaceDependencyRule>());
            _configMock.Setup(i => i.VisibleTypesByNamespace).Returns(new Dictionary<Namespace, TypeNameSet>());
            _configMock.Setup(i => i.MaxIssueCount).Returns(maxIssueCount);
            _configMock.Setup(i => i.AutoLowerMaxIssueCount).Returns(autoLowerMaxIssueCount);

            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.Enabled);
            _configProviderMock.Setup(i => i.ConfigException).Returns<Exception>(null);
            _configProviderMock.Setup(i => i.Config).Returns(_configMock.Object);
        }

        private ConfiguredDependencyAnalyzer CreateAnalyzer()
        {
            return new ConfiguredDependencyAnalyzer(
                _configProviderMock.Object,
                () => new DependencyAnalyzer(_configMock.Object, _typeDependencyEnumeratorMock.Object, traceMessageHandler: null));
        }
    }
}
