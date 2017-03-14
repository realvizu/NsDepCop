using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Factory
{
    [TestClass]
    public class DependencyAnalyzerFactoryTests : FileBasedTestsBase
    {
        [TestMethod]
        public void CreateFromXmlConfigFile_EnabledWithRoslynParser()
        {
            var configFilePath = GetTestFilePath("config.nsdepcop");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.Roslyn);
        }

        [TestMethod]
        public void CreateFromXmlConfigFile_EnabledWithRoslynParser_OverrideWithNRefactoryParser()
        {
            var configFilePath = GetTestFilePath("config.nsdepcop");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().OverrideParser(Parsers.NRefactory).CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.NRefactory);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_EnabledWithRoslynParser()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.Roslyn);
        }

        [TestMethod]
        public void CreateFromMultiLevelXmlConfigFile_EnabledWithRoslynParser_OverrideWithNRefactoryParser()
        {
            var configFilePath = GetTestFilePath("");
            var dependencyAnalyzer = new DependencyAnalyzerFactory().OverrideParser(Parsers.NRefactory).CreateFromMultiLevelXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.NRefactory);
        }
    }
}
