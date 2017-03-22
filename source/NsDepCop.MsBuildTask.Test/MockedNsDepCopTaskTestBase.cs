using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using FluentAssertions;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Abstract base class for mocked NsDepCopTask tests.
    /// </summary>
    public abstract class MockedNsDepCopTaskTestBase : NsDepCopTaskTestBase
    {
        /// <summary>
        /// Executes the test case using both analyzers.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        protected static void ExecuteTest(TestCaseSpecification specification)
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest(specification);
            nsDepCopTask.Execute().Should().Be(specification.ExpectedReturnValue);
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        /// <summary>
        /// Creates the specified expectations on a mock IBuildEngine.
        /// </summary>
        /// <param name="mockBuildEngine">The mock build engine.</param>
        /// <param name="expectedLogEntries">The expected log entry events.</param>
        /// <param name="baseDirecytory">The base directory of the source files.</param>
        protected static void ExpectEvents(IBuildEngine mockBuildEngine, IEnumerable<LogEntryParameters> expectedLogEntries,
            string baseDirecytory = null)
        {
            foreach (var expectedLogEntry in expectedLogEntries.EmptyIfNull())
            {
                switch (expectedLogEntry.IssueKind)
                {
                    case IssueKind.Info:
                        mockBuildEngine
                            .Expect(i => i.LogMessageEvent(Arg<BuildMessageEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))))
                            .Repeat.Once();
                        break;

                    case IssueKind.Warning:
                        mockBuildEngine
                            .Expect(i => i.LogWarningEvent(Arg<BuildWarningEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))))
                            .Repeat.Once();
                        break;

                    case IssueKind.Error:
                        mockBuildEngine
                            .Expect(i => i.LogErrorEvent(Arg<BuildErrorEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory))))
                            .Repeat.Once();
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
        private static NsDepCopTask SetUpNsDepCopTaskForTest(TestCaseSpecification specification)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.IsNotNull(assemblyDirectory);

            var baseDirectory = Path.Combine(assemblyDirectory, specification.TestFilesFolderName);

            var mockBuildEngine = MockRepository.GenerateStrictMock<IBuildEngine>();

            ExpectAnyDiagnosticEvents(mockBuildEngine);

            if (specification.ExpectStartEvent)
                ExpectStartEvent(mockBuildEngine);

            ExpectEvents(mockBuildEngine, specification.ExpectedLogEntries, baseDirectory);

            if (specification.ExpectEndEvent)
                ExpectEndEvent(mockBuildEngine);

            var nsDepCopTask = new NsDepCopTask
            {
                BaseDirectory = new TestTaskItem(baseDirectory),
                Compile = CreateTaskItems(CreateFullPathFileNames(baseDirectory, specification.SourceFileNames)),
                ReferencePath = CreateTaskItems(specification.ReferencedFilePaths),
                BuildEngine = mockBuildEngine,
            };

            return nsDepCopTask;
        }

        private static void ExpectAnyDiagnosticEvents(IBuildEngine mockBuildEngine)
        {
            mockBuildEngine
                .Expect(i => i.LogMessageEvent(Arg<BuildMessageEventArgs>.Matches(e => e.Importance == MessageImportance.Low)))
                .Repeat.Any();
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

        private static void ExpectStartEvent(IBuildEngine mockBuildEngine)
        {
            ExpectEvents(mockBuildEngine, new[]
            {
                new LogEntryParameters { IssueKind = NsDepCopTask.TaskStartedIssue.DefaultKind, Code = NsDepCopTask.TaskStartedIssue.Id },
            });
        }

        private static void ExpectEndEvent(IBuildEngine mockBuildEngine)
        {
            ExpectEvents(mockBuildEngine, new[]
            {
                new LogEntryParameters { IssueKind = NsDepCopTask.TaskFinishedIssue.DefaultKind, Code = NsDepCopTask.TaskFinishedIssue.Id },
            });
        }
    }
}