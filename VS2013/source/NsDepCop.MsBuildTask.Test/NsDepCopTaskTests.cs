﻿using Codartis.NsDepCop.Core.Analyzer.Factory;
using Codartis.NsDepCop.Core.Common;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SoftwareApproach.TestingExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Unit tests for the NsDepCopTask class.
    /// </summary>
    [TestClass]
    public class NsDepCopTaskTests
    {
        [TestMethod]
        public void Execute_NoConfigFile()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_NoConfigFile",
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = Constants.DIAGNOSTIC_ID_NO_CONFIG_FILE}
                },
                ExpectStartEvent = false,
                ExpectEndEvent = false,
            });
        }

        [TestMethod]
        public void Execute_NonXmlConfigFile()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_NonXmlConfigFile",
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters { IssueKind = IssueKind.Error, Code = NsDepCopTask.MSBUILD_CODE_EXCEPTION }
                },
                ExpectedReturnValue = false,
                ExpectStartEvent = false,
                ExpectEndEvent = false,
            });
        }

        [TestMethod]
        public void Execute_ConfigFileDisabled()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_ConfigDisabled",
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters { IssueKind = IssueKind.Warning, Code = Constants.DIAGNOSTIC_ID_CONFIG_DISABLED }
                },
                ExpectStartEvent = false,
                ExpectEndEvent = false
            });
        }

        [TestMethod]
        public void Execute_ConfigFileErrorsIgnored()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_ConfigFileErrorsIgnored",
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters { IssueKind = IssueKind.Warning, Code = Constants.DIAGNOSTIC_ID_CONFIG_DISABLED }
                },
                ExpectStartEvent = false,
                ExpectEndEvent = false
            });
        }

        [TestMethod]
        public void Execute_AllowedDependency()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_AllowedDependency",
                SourceFileNames = new[] { "AllowedDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_SameNamespaceAlwaysAllowed()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_SameNamespaceAlwaysAllowed",
                SourceFileNames = new[] { "SameNamespaceAlwaysAllowed.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportWarning()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportWarning",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportInfo()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportInfo.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportInfo",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Info,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportError()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportError.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportError",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Error,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_QualifiedName()
        {
            const string sourceFileName = "DepViolation_QualifiedName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_QualifiedName",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 5,
                        StartColumn = 19,
                        EndLine = 5,
                        EndColumn = 25
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_AliasQualifiedName()
        {
            const string sourceFileName = "DepViolation_AliasQualifiedName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_AliasQualifiedName",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 25,
                        EndLine = 7,
                        EndColumn = 31
                    },
                },
                SkipLocationValidation = true
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationExpression()
        {
            const string sourceFileName = "DepViolation_InvocationExpression.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationExpression",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 26,
                        EndLine = 9,
                        EndColumn = 39
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationWithTypeArg()
        {
            const string sourceFileName = "DepViolation_InvocationWithTypeArg.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationWithTypeArg",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 42,
                        EndLine = 9,
                        EndColumn = 50
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_MemberAccessExpression()
        {
            const string sourceFileName = "DepViolation_MemberAccessExpression.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_MemberAccessExpression",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 37,
                        EndLine = 9,
                        EndColumn = 47
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_GenericName()
        {
            const string sourceFileName = "DepViolation_GenericName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericName",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 31
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_GenericTypeArgument()
        {
            const string sourceFileName = "DepViolation_GenericTypeArgument.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericTypeArgument",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 32,
                        EndLine = 7,
                        EndColumn = 40
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 36,
                        EndLine = 8,
                        EndColumn = 44
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_NestedType()
        {
            const string sourceFileName = "DepViolation_NestedType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_NestedType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 25
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 26,
                        EndLine = 7,
                        EndColumn = 36
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 19,
                        EndLine = 8,
                        EndColumn = 27
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 28,
                        EndLine = 8,
                        EndColumn = 38
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ArrayType()
        {
            const string sourceFileName = "DepViolation_ArrayType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_ArrayType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 25
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_NullableType()
        {
            const string sourceFileName = "DepViolation_NullableType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_NullableType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 25
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_EveryUserDefinedTypeKind()
        {
            const string sourceFileName = "DepViolation_EveryUserDefinedTypeKind.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_EveryUserDefinedTypeKind",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 24
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 29
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 17,
                        EndLine = 9,
                        EndColumn = 25
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 10,
                        StartColumn = 17,
                        EndLine = 10,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 11,
                        StartColumn = 17,
                        EndLine = 11,
                        EndColumn = 27
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_TooManyIssues()
        {
            const string sourceFileName = "TooManyIssues.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_TooManyIssues",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 23
                    },
                    new LogEntryParameters { IssueKind = IssueKind.Warning, Code = Constants.DIAGNOSTIC_ID_TOO_MANY_ISSUES},
                },
            });
        }

        [TestMethod]
        public void Execute_NoSourceFiles()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_NoSourceFiles",
            });
        }

        [TestMethod]
        public void Execute_MultipleSourceFiles()
        {
            const string sourceFile1 = "SourceFile1.cs";
            const string sourceFile2 = "SourceFile2.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_MultipleSourceFiles",
                SourceFileNames = new[] { sourceFile1, sourceFile2 },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFile1,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFile2,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
            });
        }

        [TestMethod]
        public void Execute_AllowedDependency_ByChildCanDependOnParentImplicitlyOption()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_ByChildCanDependOnParentImplicitlyOption",
                SourceFileNames = new[] { "ChildToParentDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ExtensionMethodInvocation()
        {
            const string sourceFileName = "DepViolation_ExtensionMethodInvocation.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification()
            {
                TestFilesFolderName = "TestFiles_DepViolation_ExtensionMethodInvocation",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 27,
                        EndLine = 9,
                        EndColumn = 44
                    },
                    new LogEntryParameters
                    {
                        IssueKind = IssueKind.Warning,
                        Code = Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
                        Path = sourceFileName,
                        StartLine = 10,
                        StartColumn = 27,
                        EndLine = 10,
                        EndColumn = 51
                    },
                },
            });
        }

        /// <summary>
        /// Executes the test case using both analyzers.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        private static void ExecuteWithAllAnalyzers(TestCaseSpecification specification)
        {
            {
                Debug.WriteLine("--> Running test with NRefactory...");
                var nsDepCopTask = SetUpNsDepCopTaskForTest(specification);
                nsDepCopTask.Parser = new TestTaskItem(Parser.NRefactory.ToString());
                nsDepCopTask.Execute().ShouldEqual(specification.ExpectedReturnValue);
                nsDepCopTask.BuildEngine.VerifyAllExpectations();
            }
            {
                Debug.WriteLine("--> Running test with Roslyn...");
                var nsDepCopTask = SetUpNsDepCopTaskForTest(specification);
                nsDepCopTask.Parser = new TestTaskItem(Parser.Roslyn.ToString());
                nsDepCopTask.Execute().ShouldEqual(specification.ExpectedReturnValue);
                nsDepCopTask.BuildEngine.VerifyAllExpectations();
            }
        }

        /// <summary>
        /// Creates an NsDepCopTask and sets it up for testing. Adds a mocked BuildEngine and log expectations.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        /// <returns>A new NsDepCopTask instance ready for testing.</returns>
        private static NsDepCopTask SetUpNsDepCopTaskForTest(TestCaseSpecification specification)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDirecytory = Path.Combine(assemblyDirectory, specification.TestFilesFolderName);

            var mockBuildEngine = MockRepository.GenerateStrictMock<IBuildEngine>();

            if (specification.ExpectStartEvent)
                ExpectStartEvent(mockBuildEngine);

            ExpectEvents(mockBuildEngine, specification.ExpectedLogEntries, baseDirecytory, specification.SkipLocationValidation);

            if (specification.ExpectEndEvent)
                ExpectEndEvent(mockBuildEngine);

            var nsDepCopTask = new NsDepCopTask()
            {
                BaseDirectory = new TestTaskItem(baseDirecytory),
                Compile = CreateTaskItems(CreateFullPathFileNames(baseDirecytory, specification.SourceFileNames)),
                ReferencePath = CreateTaskItems(specification.ReferencedFilePaths),
                BuildEngine = mockBuildEngine,
            };

            return nsDepCopTask;
        }

        /// <summary>
        /// Creates a collection of full path filenames from the given filenames and a base directory.
        /// </summary>
        /// <param name="baseDirecytory">A directory.</param>
        /// <param name="sourceFileNames">A collection of filenames (without full path).</param>
        /// <returns>A collection of full path filenames.</returns>
        private static IEnumerable<string> CreateFullPathFileNames(string baseDirecytory, IEnumerable<string> sourceFileNames)
        {
            return sourceFileNames.EmptyIfNull().Select(i => FileNameToFullPath(baseDirecytory, i));
        }

        /// <summary>
        /// Creates MSBuild an TaskItem array from the given string collection by wrapping the string content into TestTaskItem objects.
        /// </summary>
        /// <param name="payloadCollection">A string collection. The payload of the task items.</param>
        /// <returns>A TaskItem array created from the given payload.</returns>
        private static TestTaskItem[] CreateTaskItems(IEnumerable<string> payloadCollection)
        {
            return payloadCollection.EmptyIfNull().Select(i => new TestTaskItem(i)).ToArray();
        }

        /// <summary>
        /// Creates the specified expectations on a mock IBuildEngine.
        /// </summary>
        /// <param name="mockBuildEngine">The mock build engine.</param>
        /// <param name="expectedLogEntries">The expected log entry events.</param>
        /// <param name="baseDirecytory">The base directory of the source files.</param>
        /// <param name="skipLocationValidation">If true then skip the validation of the location info.</param>
        private static void ExpectEvents(IBuildEngine mockBuildEngine, IEnumerable<LogEntryParameters> expectedLogEntries,
            string baseDirecytory = null, bool skipLocationValidation = true)
        {
            foreach (var expectedLogEntry in expectedLogEntries.EmptyIfNull())
            {
                switch (expectedLogEntry.IssueKind)
                {
                    case (IssueKind.Info):
                        mockBuildEngine
                            .Expect(i => i.LogMessageEvent(Arg<BuildMessageEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory, skipLocationValidation))))
                            .Repeat.Once();
                        break;

                    case (IssueKind.Warning):
                        mockBuildEngine
                            .Expect(i => i.LogWarningEvent(Arg<BuildWarningEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory, skipLocationValidation))))
                            .Repeat.Once();
                        break;

                    case (IssueKind.Error):
                        mockBuildEngine
                            .Expect(i => i.LogErrorEvent(Arg<BuildErrorEventArgs>
                                .Matches(e => LogEntryEqualsExpected(e, expectedLogEntry, baseDirecytory, skipLocationValidation))))
                            .Repeat.Once();
                        break;

                    default:
                        throw new Exception(string.Format("Unexpected IssueKind: {0}", expectedLogEntry.IssueKind));
                }
            }
        }

        /// <summary>
        /// Compares a log entry to an expected value.
        /// </summary>
        /// <param name="logEntry">The log entry to be verified.</param>
        /// <param name="expectedLogEntry">The expected log entry.</param>
        /// <param name="baseDirecytory">The base directory of the source files.</param>
        /// <param name="skipLocationValidation">If true then skip the validation of the location info.</param>
        /// <returns>True if the given log entry equals to the expected log entry.</returns>
        private static bool LogEntryEqualsExpected(dynamic logEntry, LogEntryParameters expectedLogEntry, string baseDirecytory, bool skipLocationValidation)
        {
            return logEntry.Code == expectedLogEntry.Code
                && logEntry.File == FileNameToFullPath(baseDirecytory, expectedLogEntry.Path)
                && (skipLocationValidation || LocationEqualsExpected(logEntry, expectedLogEntry));
        }

        /// <summary>
        /// Compares a location to an expected value.
        /// </summary>
        /// <param name="logEntry">The log entry to be verified.</param>
        /// <param name="expectedLogEntry">The expected log entry.</param>
        /// <returns>True if the given location equals to the expected location.</returns>
        private static bool LocationEqualsExpected(dynamic logEntry, LogEntryParameters expectedLogEntry)
        {
            return logEntry.LineNumber == expectedLogEntry.StartLine
                && logEntry.ColumnNumber == expectedLogEntry.StartColumn
                && logEntry.EndLineNumber == expectedLogEntry.EndLine
                && logEntry.EndColumnNumber == expectedLogEntry.EndColumn;
        }

        /// <summary>
        /// Registers the expectation of a start event on the given BuildEngine mock.
        /// </summary>
        /// <param name="mockBuildEngine">A mock BuildEngine.</param>
        private static void ExpectStartEvent(IBuildEngine mockBuildEngine)
        {
            ExpectEvents(mockBuildEngine, new[]
            {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
            });
        }

        /// <summary>
        /// Registers the expectation of an end event on the given BuildEngine mock.
        /// </summary>
        /// <param name="mockBuildEngine">A mock BuildEngine.</param>
        private static void ExpectEndEvent(IBuildEngine mockBuildEngine)
        {
            ExpectEvents(mockBuildEngine, new[]
            {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
            });
        }

        /// <summary>
        /// Converts a file name to full path by prepending a base directory.
        /// </summary>
        /// <param name="baseDirecytory">The full path of the base directory.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The filename converted to full path.</returns>
        private static string FileNameToFullPath(string baseDirecytory, string fileName)
        {
            return baseDirecytory != null && fileName != null
                ? Path.Combine(baseDirecytory, fileName)
                : null;
        }
    }
}
