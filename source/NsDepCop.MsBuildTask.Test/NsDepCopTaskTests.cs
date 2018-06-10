using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.TestUtil;
using FluentAssertions;
using Microsoft.Build.Framework;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Unit tests for the NsDepCopTask class.
    /// </summary>
    public class NsDepCopTaskTests : FileBasedTestsBase
    {
        private readonly Mock<ILogger> _loggerMock;

        public NsDepCopTaskTests(ITestOutputHelper output)
        {
            _loggerMock = new Mock<ILogger>();

            _loggerMock.Setup(i => i.LogError(It.IsAny<string>()))
                .Callback<string>(i => output.WriteLine($"Exception caught in task: {i}"));
        }

        [Fact]
        public void AllowedDependency()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyDependencyIssueLogged(Times.Never());
        }

        [Fact]
        public void ConfigDisabled()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyConfigDisabledLogged(Times.Once());
        }

        [Fact]
        public void ConfigFileErrorCausesException()
        {
            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyConfigExceptionLogged(Times.Once());
        }

        [Fact]
        public void ConfigFileExtraElementsIgnored()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyConfigExceptionLogged(Times.Never());
        }

        [Fact]
        public void DependencyViolation_ReportError()
        {
            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyDependencyIssueLogged(Times.Once(), IssueKind.Error);
        }

        [Fact]
        public void DependencyViolation_ReportWarning()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyDependencyIssueLogged(Times.Once(), IssueKind.Warning);
        }

        [Fact]
        public void DependencyViolation_ReportInfo()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyDependencyIssueLogged(Times.Once(), IssueKind.Info);
        }

        [Fact]
        public void ConfigInfoImportanceIsHigh()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            _loggerMock.VerifySet(i => i.InfoImportance = MessageImportance.High);
        }

        [Fact]
        public void TaskInfoImportanceIsLow()
        {
            var nsDepCopTask = CreateNsDepCopTask();
            nsDepCopTask.InfoImportance = new TestTaskItem("Low");
            nsDepCopTask.Execute().Should().BeTrue();

            _loggerMock.VerifySet(i => i.InfoImportance = MessageImportance.Low);
        }

        [Fact]
        public void IssueCountMoreThanMaxIssueCount_MaxIssueCountIssueLogged()
        {
            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyDependencyIssueLogged(Times.Exactly(2));
            VerifyTooManyIssuesLogged(Times.Once(), IssueKind.Error);
        }

        [Fact]
        public void IssueCountEqualsToMaxIssueCount_MaxIssueCountIssueNotLogged()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyDependencyIssueLogged(Times.Exactly(2));
            VerifyTooManyIssuesLogged(Times.Never());
        }

        [Fact]
        public void NoSourceFiles()
        {
            var nsDepCopTask = CreateNsDepCopTask();
            nsDepCopTask.Compile = new ITaskItem[0];
            nsDepCopTask.Execute().Should().BeTrue();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyDependencyIssueLogged(Times.Never());
        }

        [Fact]
        public void MultipleSourceFiles()
        {
            var nsDepCopTask = CreateNsDepCopTask();
            nsDepCopTask.Compile = new ITaskItem[]
            {
                new TestTaskItem(Path.Combine(nsDepCopTask.BaseDirectory.ItemSpec, "SourceFile1.cs")),
                new TestTaskItem(Path.Combine(nsDepCopTask.BaseDirectory.ItemSpec, "SourceFile2.cs"))
            };
            nsDepCopTask.Execute().Should().BeTrue();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyDependencyIssueLogged(Times.Exactly(2));
        }

        [Fact]
        public void NoConfigFile()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyNoConfigFileLogged(Times.Once());
        }

        [Fact]
        public void NonXmlConfigFile()
        {
            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyConfigExceptionLogged(Times.Once());
        }

        [Fact]
        public void UnexpectedException_TaskExceptionLogged()
        {
            _loggerMock.Setup(i => i.LogTraceMessage(It.IsAny<IEnumerable<string>>())).Throws<Exception>();

            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyTaskExceptionLogged(Times.Once());
        }

        [Fact]
        public void Serialization()
        {
            CreateNsDepCopTask().Execute().Should().BeFalse();

            VerifyTaskExceptionLogged(Times.Never());
            VerifyDependencyIssueLogged(Times.Exactly(3));
        }

        private NsDepCopTask CreateNsDepCopTask([CallerMemberName] string taskName = null)
        {
            var testFileFullPath = GetTestFileFullPath(taskName);
            return new NsDepCopTask(_loggerMock.Object)
            {
                BaseDirectory = new TestTaskItem(Path.GetDirectoryName(testFileFullPath)),
                Compile = new ITaskItem[] {new TestTaskItem(testFileFullPath)},
                ReferencePath = GetReferencedAssemblyPaths().Select(i => new TestTaskItem(i)).OfType<ITaskItem>().ToArray()
            };
        }

        private static string GetTestFileFullPath(string testName)
        {
            return Path.Combine(GetBinFilePath($@"{testName}\{testName}.cs"));
        }

        private static IEnumerable<string> GetReferencedAssemblyPaths()
        {
            return new[]
            {
                // mscorlib
                GetAssemblyPath(typeof(object).Assembly),
            };
        }

        private void VerifyDependencyIssueLogged(Times times, IssueKind? issueKind = null)
            => _loggerMock.Verify(i => i.LogIssue(It.Is<IllegalDependencyMessage>(m => issueKind == null || m.IssueKind == issueKind)), times);

        private void VerifyTaskExceptionLogged(Times times)
            => _loggerMock.Verify(i => i.LogError(It.IsAny<string>()), times);

        private void VerifyConfigExceptionLogged(Times times)
            => _loggerMock.Verify(i => i.LogIssue(It.IsAny<ConfigErrorMessage>()), times);

        private void VerifyTooManyIssuesLogged(Times times, IssueKind? issueKind = null)
            => _loggerMock.Verify(i => i.LogIssue(It.Is<TooManyIssuesMessage>(m => m.IssueKind == issueKind)), times);

        private void VerifyNoConfigFileLogged(Times times)
            => _loggerMock.Verify(i => i.LogIssue(It.IsAny<NoConfigFileMessage>()), times);

        private void VerifyConfigDisabledLogged(Times times)
            => _loggerMock.Verify(i => i.LogIssue(It.IsAny<ConfigDisabledMessage>()), times);
    }
}