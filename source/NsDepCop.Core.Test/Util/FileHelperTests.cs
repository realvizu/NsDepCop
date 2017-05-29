using System;
using System.Linq;
using Codartis.NsDepCop.Core.Util;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Xunit;

namespace Codartis.NsDepCop.Core.Test.Util
{
    public class FileHelperTests : FileBasedTestsBase
    {
        [Fact]
        public void GetFolderPaths_MaxLevelsIsZero_ThrowsArgumentException()
        {
            Action a = () => FileHelper.GetFolderPaths(GetTestFilePath("whatever"), 0).ToList();
            a.ShouldThrow<ArgumentException>().WithMessage("*must be > 0*");
        }

        [Fact]
        public void GetFolderPaths_NonExistingFolder_ThrowsArgumentException()
        {
            Action a = () => FileHelper.GetFolderPaths(GetTestFilePath(@"NoSuchFolder"), 3).ToList();
            a.ShouldThrow<ArgumentException>().WithMessage("*Starting folder does not exist*");
        }

        [Fact]
        public void GetFolderPaths_Works()
        {
            var folderPaths = FileHelper.GetFolderPaths(GetTestFilePath(@"Level3\Level2\Level1"), 3).ToList();
            folderPaths.Should().HaveCount(3);
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1"));
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3\Level2"));
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3"));
        }

        [Fact]
        public void GetFilenameWithFolderPaths_Works()
        {
            var folderPaths = FileHelper.GetFilenameWithFolderPaths("a.txt", GetTestFilePath(@"Level3\Level2\Level1"), 3).ToList();
            folderPaths.Should().HaveCount(3);
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1\a.txt"));
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\a.txt"));
            folderPaths.Should().Contain(i => i.EndsWith(@"Level3\a.txt"));
        }

        [Fact]
        public void FindInParentFolders_Max1Level()
        {
            var filePaths = FileHelper.FindInParentFolders("test.txt", GetTestFilePath(@"Level3\Level2\Level1"), 1).ToList();
            filePaths.Should().HaveCount(1);
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1\test.txt"));
        }

        [Fact]
        public void FindInParentFolders_Max3Levels()
        {
            var filePaths = FileHelper.FindInParentFolders("test.txt", GetTestFilePath(@"Level3\Level2\Level1"), 3).ToList();
            filePaths.Should().HaveCount(2);
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\Level2\Level1\test.txt"));
            filePaths.Should().Contain(i => i.EndsWith(@"Level3\test.txt"));
        }
    }
}
