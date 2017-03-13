using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    [TestClass]
    public class CachingCsprojResolverTests
    {
        private Mock<ICsprojResolver> _embeddedCsprojResolverMock;

        [TestInitialize]
        public void TestInitialize()
        {
            _embeddedCsprojResolverMock = new Mock<ICsprojResolver>();
        }

        [TestMethod]
        public void GetCsprojFile_RetrievedTwice_ButEmbeddedResolverCalledOnlyOnce()
        {
            const string sourceFilePath = "mysource";
            const string assemblyName = "myassembly";

            var cachingCsprojResolver = CreateCsprojResolver();

            cachingCsprojResolver.GetCsprojFile(sourceFilePath, assemblyName);
            _embeddedCsprojResolverMock.Verify(i => i.GetCsprojFile(sourceFilePath, assemblyName), Times.Once);

            cachingCsprojResolver.GetCsprojFile(sourceFilePath, assemblyName);
            _embeddedCsprojResolverMock.Verify(i => i.GetCsprojFile(sourceFilePath, assemblyName), Times.Once);
        }

        private ICsprojResolver CreateCsprojResolver()
        {
            return new CachingCsprojResolver(_embeddedCsprojResolverMock.Object);
        }
    }
}
