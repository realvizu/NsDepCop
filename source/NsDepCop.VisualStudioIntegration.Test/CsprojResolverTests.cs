using System.Diagnostics;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    [TestClass]
    public class CsprojResolverTests : FileBasedTestsBase
    {
        private const string TestAssemblyName = "MyAssembly";

        [TestMethod]
        public void GetCsprojFile_FoundInSameFolder()
        {
            var csprojResolver = CreateCsprojResolver();
            var sourceFilePath = GetTestFilePath(@"GetCsprojFile_FoundInSameFolder\dummy.cs");
            var expectedResult = GetTestFilePath(@"GetCsprojFile_FoundInSameFolder\matching.csproj");
            csprojResolver.GetCsprojFile(sourceFilePath, TestAssemblyName).Should().Be(expectedResult);
        }

        [TestMethod]
        public void GetCsprojFile_FoundOneFolderHigher()
        {
            var csprojResolver = CreateCsprojResolver();
            var sourceFilePath = GetTestFilePath(@"GetCsprojFile_FoundOneFolderHigher\Level1\dummy.cs");
            var expectedResult = GetTestFilePath(@"GetCsprojFile_FoundOneFolderHigher\matching.csproj");
            csprojResolver.GetCsprojFile(sourceFilePath, TestAssemblyName).Should().Be(expectedResult);
        }

        [TestMethod]
        public void GetCsprojFile_NotFound_ReturnsNull()
        {
            var csprojResolver = CreateCsprojResolver();
            var sourceFilePath = GetTestFilePath(@"GetCsprojFile_NotFound\dummy.cs");
            csprojResolver.GetCsprojFile(sourceFilePath, TestAssemblyName).Should().BeNull();
        }

        [TestMethod]
        public void GetCsprojFile_NotExistingFolder_ReturnsNull()
        {
            var csprojResolver = CreateCsprojResolver();
            var sourceFilePath = GetTestFilePath(@"NotExistingFolder\dummy.cs");
            csprojResolver.GetCsprojFile(sourceFilePath, TestAssemblyName).Should().BeNull();
        }

        private static CsprojResolver CreateCsprojResolver()
        {
            return new CsprojResolver(i => Debug.WriteLine(i));
        }
    }
}
