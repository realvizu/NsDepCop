using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using FluentAssertions;
using Microsoft.Build.Framework;
using Moq;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Abstract base class for mocked NsDepCopTask tests.
    /// </summary>
    public abstract class MockedNsDepCopTaskTestBase : NsDepCopTaskTestBase
    {
        protected readonly Mock<IBuildEngine> BuildEngineMock = new Mock<IBuildEngine>();

        /// <summary>
        /// Executes the test case using both analyzers.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        protected void ExecuteTest(TestCaseSpecification specification)
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest(specification);
            nsDepCopTask.Execute().Should().Be(specification.ExpectedReturnValue);
        }

        /// <summary>
        /// Creates the specified expectations on a mock IBuildEngine.
        /// </summary>
        /// <param name="expectedLogEntries">The expected log entry events.</param>
        /// <param name="baseDirecytory">The base directory of the source files.</param>
        protected void ExpectEvents(IEnumerable<LogEntryParameters> expectedLogEntries,
            string baseDirecytory = null)
        {
            foreach (var expectedLogEntry in expectedLogEntries.EmptyIfNull())
            {
                switch (expectedLogEntry.IssueKind)
                {
                    case IssueKind.Info:
                        BuildEngineMock.Setup(i => i.LogMessageEvent(
                            It.Is<BuildMessageEventArgs>(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))));
                        break;

                    case IssueKind.Warning:
                        BuildEngineMock.Setup(i => i.LogWarningEvent(
                            It.Is<BuildWarningEventArgs>(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))));
                        break;

                    case IssueKind.Error:
                        BuildEngineMock.Setup(i => i.LogErrorEvent(
                            It.Is<BuildErrorEventArgs>(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))));
                        break;

                    default:
                        throw new Exception($"Unexpected IssueKind: {expectedLogEntry.IssueKind}");
                }
            }
        }

        protected static LogEntryParameters CreateLogEntryParameters(string sourceFileName, int startLine, int startColumn, int endLine, int endColumn,
            IssueKind issueKind = IssueKind.Warning, string code = null)
        {
            return new LogEntryParameters
            {
                Code = code ?? IssueDefinitions.IllegalDependencyIssue.Id,
                IssueKind = issueKind,
                Path = sourceFileName,
                StartLine = startLine,
                StartColumn = startColumn,
                EndLine = endLine,
                EndColumn = endColumn
            };
        }

        /// <summary>
        /// Creates an NsDepCopTask and sets it up for testing. Adds a mocked BuildEngine and log expectations.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        /// <returns>A new NsDepCopTask instance ready for testing.</returns>
        private NsDepCopTask SetUpNsDepCopTaskForTest(TestCaseSpecification specification)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDirectory = Path.Combine(assemblyDirectory, specification.TestFilesFolderName);

            if (specification.ExpectStartEvent)
                ExpectStartEvent();

            ExpectEvents(specification.ExpectedLogEntries, baseDirectory);

            if (specification.ExpectEndEvent)
                ExpectEndEvent();

            var nsDepCopTask = new NsDepCopTask
            {
                BaseDirectory = new TestTaskItem(baseDirectory),
                Compile = CreateTaskItems(CreateFullPathFileNames(baseDirectory, specification.SourceFileNames)),
                ReferencePath = CreateTaskItems(CreateFullPathFileNames(assemblyDirectory, specification.ReferencedFilePaths)),
                BuildEngine = BuildEngineMock.Object,
            };

            return nsDepCopTask;
        }

        private static bool LogEntryEqualsExpected(dynamic logEntry, LogEntryParameters expectedLogEntry, string baseDirecytory)
        {
            return LogEntryHasExpectedCode(logEntry, expectedLogEntry)
                   && LogEntryHasExpectedFile(logEntry, expectedLogEntry, baseDirecytory)
                   && LogEntryHasExpectedImportance(logEntry, expectedLogEntry)
                   && LogEntryHasExpectedLocation(logEntry, expectedLogEntry);
        }

        private static bool LogEntryHasExpectedCode(dynamic logEntry, LogEntryParameters expectedLogEntry)
        {
            return logEntry.Code == expectedLogEntry.Code;
        }

        private static bool LogEntryHasExpectedFile(dynamic logEntry, LogEntryParameters expectedLogEntry, string baseDirecytory)
        {
            return logEntry.File == FileNameToFullPath(baseDirecytory, expectedLogEntry.Path);
        }

        private static bool LogEntryHasExpectedImportance(dynamic logEntry, LogEntryParameters expectedLogEntry)
        {
            return !(logEntry is BuildMessageEventArgs)
                   || !expectedLogEntry.InfoImportance.HasValue
                   || expectedLogEntry.InfoImportance.Value.ToMessageImportance() == logEntry.Importance;
        }

        private static bool LogEntryHasExpectedLocation(dynamic logEntry, LogEntryParameters expectedLogEntry)
        {
            return LocationEqualsExpected(logEntry, expectedLogEntry);
        }

        private static bool LocationEqualsExpected(dynamic logEntry, LogEntryParameters expectedLogEntry)
        {
            return logEntry.LineNumber == expectedLogEntry.StartLine
                   && logEntry.ColumnNumber == expectedLogEntry.StartColumn
                   && logEntry.EndLineNumber == expectedLogEntry.EndLine
                   && logEntry.EndColumnNumber == expectedLogEntry.EndColumn;
        }

        private void ExpectStartEvent()
        {
            ExpectEvents(new[]
            {
                new LogEntryParameters { IssueKind = NsDepCopTask.TaskStartedIssue.DefaultKind, Code = NsDepCopTask.TaskStartedIssue.Id },
            });
        }

        private void ExpectEndEvent()
        {
            ExpectEvents(new[]
            {
                new LogEntryParameters { IssueKind = NsDepCopTask.TaskFinishedIssue.DefaultKind, Code = NsDepCopTask.TaskFinishedIssue.Id },
            });
        }
    }
}