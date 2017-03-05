using Codartis.NsDepCop.Core.Util;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Util
{
    [TestClass]
    public class FileFinderTests : FileBasedTestsBase
    {
        [TestMethod]
        public void FindInParentFolders_Max1Level()
        {
            var filePaths = FileFinder.FindInParentFolders("test.txt", GetTestFilePath(@"Level3\Level2\Level1"), 1);
            filePaths.Should().HaveCount(1);
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1\test.txt"));
        }

        [TestMethod]
        public void FindInParentFolders_Max3Levels()
        {
            var filePaths = FileFinder.FindInParentFolders("test.txt", GetTestFilePath(@"Level3\Level2\Level1"), 3);
            filePaths.Should().HaveCount(2);
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1\test.txt"));
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\test.txt"));
        }

        [TestMethod]
        public void FindInParentFolders_NonExistingFolder_NoException()
        {
            var filePaths = FileFinder.FindInParentFolders("test.txt", GetTestFilePath(@"NoSuchFolder"), 3);
            filePaths.Should().HaveCount(0);
        }
    }
}
