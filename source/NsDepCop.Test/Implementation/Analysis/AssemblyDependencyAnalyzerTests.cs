using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Implementation;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Moq;
using Xunit;

namespace Codartis.NsDepCop.Test.Implementation.Analysis
{
    public class AssemblyDependencyAnalyzerTests
    {
        private static AssemblyIdentity SourceAssembly = AssemblyIdentity.FromAssemblyDefinition(typeof(AssemblyDependencyAnalyzerTests).Assembly);
        private static AssemblyIdentity ReferencedAssemblyOne = AssemblyIdentity.FromAssemblyDefinition(typeof(string).Assembly);
        private static AssemblyIdentity ReferencedAssemblyTwo = AssemblyIdentity.FromAssemblyDefinition(typeof(Assert).Assembly);

        private readonly Mock<IUpdateableConfigProvider> _configProviderMock = new();
        private readonly Mock<IAnalyzerConfig> _configMock = new();

        [Fact]
        public void NoConfig_ReturnsNoConfigMessage()
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.NoConfig);

            AnalyzeProject().OfType<NoConfigFileMessage>().Should().HaveCount(1);
        }

        [Fact]
        public void ConfigDisabled_ReturnsConfigDisabledMessage()
        {
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.Disabled);

            AnalyzeProject().OfType<ConfigDisabledMessage>().Should().HaveCount(1);
        }

        [Fact]
        public void ConfigError_ReturnsConfigErrorMessage()
        {
            var theException = new Exception();
            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.ConfigError);
            _configProviderMock.Setup(i => i.ConfigException).Returns(theException);

            var configErrorMessages = AnalyzeProject().OfType<ConfigErrorMessage>().ToList();
            configErrorMessages.Should().HaveCount(1);
            configErrorMessages.First().Exception.Should().Be(theException);
        }

        [Fact]
        public void AnalyzeProject_TwoIllegalAssemblyDependencyMessagesReturned()
        {
            // Arrange
            SetUpEnabledConfig();

            // Act
            var actual = AnalyzeProject().OfType<IllegalAssemblyDependencyMessage>();

            // Assert
            actual.Should().HaveCount(2);
        }

        [Fact]
        public void AnalyzeProject_NoIllegalAssemblyDependencyMessageReturned()
        {
            // Arrange
            SetUpEnabledConfig(checkAssemblyDependencies: false);

            // Act
            var actual = AnalyzeProject().OfType<ConfigDisabledMessage>();

            // Assert
            actual.Should().HaveCount(1);
        }

        [Fact]
        public void AnalyzeProject_NoIllegalAssemblyDependencyMessageReturnedWhenAllowedAssemblyRulesDefined()
        {
            // Arrange
            SetUpEnabledConfig();
            _configMock.Setup(i => i.AllowedAssemblyRules).Returns([
                new DependencyRule(
                    from: DomainSpecificationParser.Parse(SourceAssembly.Name),
                    to: DomainSpecificationParser.Parse(ReferencedAssemblyOne.Name)
                ),
                new DependencyRule(
                    from: DomainSpecificationParser.Parse(SourceAssembly.Name),
                    to: DomainSpecificationParser.Parse(ReferencedAssemblyTwo.Name)
                )
            ]);

            // Act
            var actual = AnalyzeProject().OfType<IllegalAssemblyDependencyMessage>();

            // Assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public void AnalyzeProject_OneIllegalAssemblyDependencyMessageReturnedWhenDisallowedAssemblyRulesDefined()
        {
            // Arrange
            SetUpEnabledConfig();
            _configMock.Setup(i => i.AllowedAssemblyRules).Returns([
                new DependencyRule(
                    from: DomainSpecificationParser.Parse("*"),
                    to: DomainSpecificationParser.Parse("*")
                )
            ]);
            _configMock.Setup(i => i.DisallowedAssemblyRules).Returns([
                new DependencyRule(
                    from: DomainSpecificationParser.Parse(SourceAssembly.Name),
                    to: DomainSpecificationParser.Parse(ReferencedAssemblyTwo.Name)
                )
            ]);

            // Act
            var actual = AnalyzeProject().OfType<IllegalAssemblyDependencyMessage>();

            // Assert
            actual.Should().HaveCount(1);
        }

        private void SetUpEnabledConfig(bool checkAssemblyDependencies = true)
        {
            _configMock.Setup(i => i.AllowedAssemblyRules).Returns(new HashSet<DependencyRule>());
            _configMock.Setup(i => i.DisallowedAssemblyRules).Returns(new HashSet<DependencyRule>());
            _configMock.Setup(i => i.CheckAssemblyDependencies).Returns(checkAssemblyDependencies);

            _configProviderMock.Setup(i => i.ConfigState).Returns(AnalyzerConfigState.Enabled);
            _configProviderMock.Setup(i => i.ConfigException).Returns<Exception>(null);
            _configProviderMock.Setup(i => i.Config).Returns(_configMock.Object);
        }

        private IEnumerable<AnalyzerMessageBase> AnalyzeProject()
        {
            return CreateDependencyAnalyzer().AnalyzeProject(
                SourceAssembly, [ReferencedAssemblyOne, ReferencedAssemblyTwo]
            );
        }

        private IAssemblyDependencyAnalyzer CreateDependencyAnalyzer()
        {
            return new AssemblyDependencyAnalyzer(_configProviderMock.Object);
        }
    }
}