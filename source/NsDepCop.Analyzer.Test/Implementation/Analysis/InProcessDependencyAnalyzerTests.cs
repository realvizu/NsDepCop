using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using DotNet.Globbing;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Analyzer.Test.Implementation.Analysis
{
    public class InProcessDependencyAnalyzerTests
    {
        private static readonly SourceSegment DummySourceSegment = new SourceSegment(1, 1, 1, 1, null, null);

        private readonly Mock<IUpdateableConfigProvider> _configProviderMock = new Mock<IUpdateableConfigProvider>();
        private readonly Mock<IAnalyzerConfig> _configMock = new Mock<IAnalyzerConfig>();
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Fact]
        public void NoConfig_ReturnsNoConfigMessage()
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.NoConfig);

            AnalyzeProject().OfType<NoConfigFileMessage>().Should().HaveCount(1);
            AnalyzeSyntaxNode().OfType<NoConfigFileMessage>().Should().HaveCount(1);
        }

        [Fact]
        public void ConfigDisabled_ReturnsConfigDisabledMessage()
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.Disabled);

            AnalyzeProject().OfType<ConfigDisabledMessage>().Should().HaveCount(1);
            AnalyzeSyntaxNode().OfType<ConfigDisabledMessage>().Should().HaveCount(1);
        }

        [Fact]
        public void ConfigError_ReturnsConfigErrorMessage()
        {
            var theException = new Exception();
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.ConfigError);
            _configProviderMock.Setup(i => i.ConfigException).Returns(theException);

            {
                var configErrorMessages = AnalyzeProject().OfType<ConfigErrorMessage>().ToList();
                configErrorMessages.Should().HaveCount(1);
                configErrorMessages.First().Exception.Should().Be(theException);
            }
            {
                var configErrorMessages = AnalyzeSyntaxNode().OfType<ConfigErrorMessage>().ToList();
                configErrorMessages.Should().HaveCount(1);
                configErrorMessages.First().Exception.Should().Be(theException);
            }
        }

        [Fact]
        public void AnalyzeProject_TypeDependenciesReturned()
        {
            SetUpEnabledConfig();

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Glob>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 2));

            AnalyzeProject().OfType<IllegalDependencyMessage>().Should().HaveCount(2);
        }

        [Fact]
        public void AnalyzeSyntaxNode_TypeDependenciesReturned()
        {
            SetUpEnabledConfig();

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<ISyntaxNode>(), It.IsAny<ISemanticModel>(), It.IsAny<IEnumerable<Glob>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 2));

            AnalyzeSyntaxNode().OfType<IllegalDependencyMessage>().Should().HaveCount(2);
        }

        [Fact]
        public void RefreshConfig_Works()
        {
            SetUpEnabledConfig(maxIssueCount: 2);

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Glob>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 10));

            var dependencyAnalyzer = CreateDependencyAnalyzer();
            dependencyAnalyzer.AnalyzeProject(new List<string>(), new List<string>()).OfType<IllegalDependencyMessage>().Should().HaveCount(2);

            SetUpEnabledConfig(maxIssueCount: 4);
            dependencyAnalyzer.RefreshConfig();
            dependencyAnalyzer.AnalyzeProject(new List<string>(), new List<string>()).OfType<IllegalDependencyMessage>().Should().HaveCount(4);
        }

        [Theory]
        [InlineData(3, 2, true)]
        [InlineData(3, 3, false)]
        [InlineData(3, 4, false)]
        public void AutoLowerMaxIssueCount_Works(int maxIssueCount, int actualIssueCount, bool isUpdateCalled)
        {
            SetUpEnabledConfig(maxIssueCount: maxIssueCount, autoLowerMaxIssueCount: true);

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<Glob>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), actualIssueCount));

            var expectedIssueCount = Math.Min(actualIssueCount, maxIssueCount);

            AnalyzeProject().OfType<IllegalDependencyMessage>().Should().HaveCount(expectedIssueCount);

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

        private IEnumerable<AnalyzerMessageBase> AnalyzeProject()
        {
            return CreateDependencyAnalyzer().AnalyzeProject(new List<string>(), new List<string>());
        }

        private IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode()
        {
            return CreateDependencyAnalyzer().AnalyzeSyntaxNode(new DummySyntaxNode(), new DummySemanticModel());
        }

        private IDependencyAnalyzer CreateDependencyAnalyzer()
        {
            return new InProcessDependencyAnalyzer(_configProviderMock.Object, _typeDependencyEnumeratorMock.Object, traceMessageHandler: null);
        }

        private class DummySyntaxNode : ISyntaxNode
        {
        }

        private class DummySemanticModel : ISemanticModel
        {
        }
    }
}