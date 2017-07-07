using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Implementation.Analysis
{
    public class DependencyAnalyzerTests
    {
        private static readonly SourceSegment DummySourceSegment = new SourceSegment(1, 1, 1, 1, null, null);

        private readonly Mock<IAnalyzerConfig> _configMock = new Mock<IAnalyzerConfig>();
        private readonly Mock<ITypeDependencyEnumerator> _typeDependencyEnumeratorMock = new Mock<ITypeDependencyEnumerator>();

        [Fact]
        public void AnalyzeSyntaxNode_NoTypeDependencies_EmptyResult()
        {
            SetUpConfig();

            var dependencyAnalyzer = CreateDependencyAnalyzer();
            dependencyAnalyzer.AnalyzeSyntaxNode(null, null).Should().BeEmpty();
        }

        [Fact]
        public void AnalyzeProject_NoTypeDependencies_EmptyResult()
        {
            SetUpConfig();

            var dependencyAnalyzer = CreateDependencyAnalyzer();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().BeEmpty();
        }

        [Fact]
        public void AnalyzeProject_TypeDependenciesReturned()
        {
            SetUpConfig();

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 2));

            var dependencyAnalyzer = CreateDependencyAnalyzer();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().HaveCount(2);
        }

        [Fact]
        public void AnalyzeProject_MaxIssueCountHonored()
        {
            SetUpConfig(maxIssueCount: 2);

            _typeDependencyEnumeratorMock
                .Setup(i => i.GetTypeDependencies(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Returns(Enumerable.Repeat(new TypeDependency("N1", "T1", "N2", "T2", DummySourceSegment), 3));

            var dependencyAnalyzer = CreateDependencyAnalyzer();
            dependencyAnalyzer.AnalyzeProject(null, null).Should().HaveCount(2);
        }

        private void SetUpConfig(int maxIssueCount = 100)
        {
            _configMock.Setup(i => i.AllowRules).Returns(new Dictionary<NamespaceDependencyRule, TypeNameSet>());
            _configMock.Setup(i => i.DisallowRules).Returns(new HashSet<NamespaceDependencyRule>());
            _configMock.Setup(i => i.VisibleTypesByNamespace).Returns(new Dictionary<Namespace, TypeNameSet>());
            _configMock.Setup(i => i.MaxIssueCount).Returns(maxIssueCount);
        }

        private DependencyAnalyzer CreateDependencyAnalyzer()
            => new DependencyAnalyzer(_configMock.Object, _typeDependencyEnumeratorMock.Object, traceMessageHandler: null);
    }
}
