using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Config;
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
            var configFilePath = GetTestFilePath("RoslynParser.nsdepcop");
            var dependencyAnalyzer = DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFilePath);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.Roslyn);
        }

        [TestMethod]
        public void CreateFromXmlConfigFile_EnabledWithRoslynParser_OverrideWithNRefactoryParser()
        {
            var configFilePath = GetTestFilePath("RoslynParser.nsdepcop");
            var dependencyAnalyzer = DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFilePath, Parsers.NRefactory);
            dependencyAnalyzer.ConfigState.Should().Be(AnalyzerConfigState.Enabled);
            dependencyAnalyzer.Config.Parser.Should().Be(Parsers.NRefactory);
        }
    }
}
