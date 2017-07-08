using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Core.Interface.Analysis;
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

            _loggerMock.Setup(i => i.LogIssue(NsDepCopTask.TaskExceptionIssue, It.IsAny<Exception>(), null, null))
                .Callback<IssueDescriptor<Exception>, Exception, IssueKind?, SourceSegment?>(
                    (i1, i2, i3, i4) => output.WriteLine($"Exception caught in task: {i2}"));
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

            _loggerMock.Verify(i => i.LogIssue(IssueDefinitions.ConfigDisabledIssue));
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
        public void TooManyIssues()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyDependencyIssueLogged(Times.Exactly(2));
            _loggerMock.Verify(i => i.LogIssue(IssueDefinitions.TooManyIssuesIssue), Times.Once);
        }

        [Fact]
        public void MaxIssueCountEqualsIssueCount()
        {
            CreateNsDepCopTask().Execute().Should().BeTrue();

            VerifyDependencyIssueLogged(Times.Exactly(2));
            _loggerMock.Verify(i => i.LogIssue(IssueDefinitions.TooManyIssuesIssue), Times.Once);
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
            _loggerMock.Verify(i => i.LogIssue(IssueDefinitions.NoConfigFileIssue));
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
                Compile = new ITaskItem[] { new TestTaskItem(testFileFullPath) },
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

        private void VerifyDependencyIssueLogged(Times times) =>
            _loggerMock.Verify(
                i => i.LogIssue(
                    IssueDefinitions.IllegalDependencyIssue,
                    It.IsAny<TypeDependency>(),
                    It.IsAny<IssueKind?>(),
                    It.IsAny<SourceSegment>()),
                times);

        private void VerifyDependencyIssueLogged(Times times, IssueKind? issueKind) =>
            _loggerMock.Verify(
                i => i.LogIssue(
                    IssueDefinitions.IllegalDependencyIssue,
                    It.IsAny<TypeDependency>(),
                    issueKind,
                    It.IsAny<SourceSegment>()),
                times);

        private void VerifyTaskExceptionLogged(Times times) =>
            _loggerMock.Verify(i => i.LogIssue(NsDepCopTask.TaskExceptionIssue, It.IsAny<Exception>(), null, null), times);

        private void VerifyConfigExceptionLogged(Times times) =>
            _loggerMock.Verify(i => i.LogIssue(IssueDefinitions.ConfigExceptionIssue, It.IsAny<Exception>(), null, null), times);
    }
}
