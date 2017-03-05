using Codartis.NsDepCop.Core.Implementation.Config;
using Codartis.NsDepCop.Core.Interface.Config;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    [TestClass]
    public class MultiLevelXmlFileConfigProviderTests : FileBasedTestsBase
    {
        [TestMethod]
        public void Rules_Merged()
        {
            var folder = GetTestFilePath(@"Rules_Merged\Level2\Level1");
            var configProvider = new MultiLevelXmlFileConfigProvider(folder);

            configProvider.State.Should().Be(AnalyzerState.Enabled);
            configProvider.ConfigException.Should().BeNull();

            var allowedRules = configProvider.Config.AllowRules;
            allowedRules.Should().HaveCount(2);
            allowedRules.Keys.Should().Contain(new NamespaceDependencyRule("N1", "N2"));
            allowedRules.Keys.Should().Contain(new NamespaceDependencyRule("N3", "N4"));
        }

        [TestMethod]
        public void Attributes_LowerLevelWins()
        {
            var folder = GetTestFilePath(@"Attributes_LowerLevelWins\Level2\Level1");
            var configProvider = new MultiLevelXmlFileConfigProvider(folder);
            configProvider.Config.IssueKind.Should().Be(IssueKind.Info);
        }

        // TODO: fix
        [TestMethod, Ignore]
        public void Attributes_MissingDoesNotOverwrite()
        {
            var folder = GetTestFilePath(@"Attributes_MissingDoesNotOverwrite\Level2\Level1");
            var configProvider = new MultiLevelXmlFileConfigProvider(folder);
            configProvider.Config.IssueKind.Should().Be(IssueKind.Error);
        }
    }
}
