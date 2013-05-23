using Codartis.NsDepCop.Core;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SoftwareApproach.TestingExtensions;
using System;
using System.Collections.Generic;
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
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_NoConfigFile",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_NO_CONFIG_FILE } });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_ConfigFileDisabled()
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_ConfigDisabled",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_CONFIG_DISABLED } });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_ConfigFileErrorsIgnored()
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_ConfigFileErrorsIgnored",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_CONFIG_DISABLED } });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_AllowedDependency()
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_AllowedDependency",
                new LogEntryParameters[] { },
                new string[] { "AllowedDependency.cs" });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_SameNamespaceAlwaysAllowed()
        {
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_SameNamespaceAlwaysAllowed",
                new LogEntryParameters[] { },
                new string[] { "SameNamespaceAlwaysAllowed.cs" });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportWarning()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_IdentifierName_ReportWarning",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportInfo()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportInfo.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_IdentifierName_ReportInfo",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Info, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportError()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportError.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_IdentifierName_ReportError",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Error, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_QualifiedName()
        {
            var sourceFileName = "DepViolation_QualifiedName.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_QualifiedName",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 5,
                        StartColumn = 17,
                        EndLine = 5,
                        EndColumn = 25
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_AliasQualifiedName()
        {
            var sourceFileName = "DepViolation_AliasQualifiedName.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_AliasQualifiedName",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 31
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationExpression()
        {
            var sourceFileName = "DepViolation_InvocationExpression.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_InvocationExpression",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 13,
                        EndLine = 9,
                        EndColumn = 41
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_MemberAccessExpression()
        {
            var sourceFileName = "DepViolation_MemberAccessExpression.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_MemberAccessExpression",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 37,
                        EndLine = 9,
                        EndColumn = 47
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_GenericName()
        {
            var sourceFileName = "DepViolation_GenericName.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_GenericName",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 41
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_GenericTypeArgument()
        {
            var sourceFileName = "DepViolation_GenericTypeArgument.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_GenericTypeArgument",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 32,
                        EndLine = 7,
                        EndColumn = 40
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_ArrayType()
        {
            var sourceFileName = "DepViolation_ArrayType.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_ArrayType",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 25
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_DepViolation_NullableType()
        {
            var sourceFileName = "DepViolation_NullableType.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_DepViolation_NullableType",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 25
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        [TestMethod]
        public void Execute_TooManyIssues()
        {
            var sourceFileName = "TooManyIssues.cs";
            var nsDepCopTask = SetUpNsDepCopTaskForTest("TestFiles_TooManyIssues",
                new[] 
                {
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_TOO_MANY_ISSUES,
                    },
                },
                new string[] { sourceFileName });

            nsDepCopTask.Execute().ShouldBeTrue();
            nsDepCopTask.BuildEngine.VerifyAllExpectations();
        }

        /// <summary>
        /// Creates an NsDepCopTask and sets it up for testing. Adds a mocked BuildEngine and log expectations.
        /// </summary>
        /// <param name="testFilesFolderName">The name of the test files folder.</param>
        /// <param name="expectedLogEntries">A collection of expected log entry parameters.</param>
        /// <param name="sourceFileNames">A collection of name of the source files in the project to be analyzed.</param>
        /// <param name="referencedFilePaths">A collection of the full path of the files referenced in the project.</param>
        /// <returns>A new NsDepCopTask instance ready for testing.</returns>
        private static NsDepCopTask SetUpNsDepCopTaskForTest(
            string testFilesFolderName, 
            IEnumerable<LogEntryParameters> expectedLogEntries, 
            IEnumerable<string> sourceFileNames = null,
            IEnumerable<string> referencedFilePaths = null)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDirecytory = Path.Combine(assemblyDirectory, testFilesFolderName);

            var mockBuildEngine = MockRepository.GenerateStrictMock<IBuildEngine>();
            foreach (var expectedLogEntry in expectedLogEntries)
            {
                switch(expectedLogEntry.IssueKind)
                {
                    case(IssueKind.Info):
                        mockBuildEngine
                            .Expect(i => i.LogMessageEvent(Arg<BuildMessageEventArgs>.Matches(a => 
                                a.Code == expectedLogEntry.Code &&
                                a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path) &&
                                a.LineNumber == expectedLogEntry.StartLine &&
                                a.ColumnNumber == expectedLogEntry.StartColumn &&
                                a.EndLineNumber == expectedLogEntry.EndLine &&
                                a.EndColumnNumber == expectedLogEntry.EndColumn
                                )));
                        break;

                    case (IssueKind.Warning):
                        mockBuildEngine
                            .Expect(i => i.LogWarningEvent(Arg<BuildWarningEventArgs>.Matches(a =>
                                a.Code == expectedLogEntry.Code &&
                                a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path) &&
                                a.LineNumber == expectedLogEntry.StartLine &&
                                a.ColumnNumber == expectedLogEntry.StartColumn &&
                                a.EndLineNumber == expectedLogEntry.EndLine &&
                                a.EndColumnNumber == expectedLogEntry.EndColumn
                                )));
                        break;

                    case (IssueKind.Error):
                        mockBuildEngine
                            .Expect(i => i.LogErrorEvent(Arg<BuildErrorEventArgs>.Matches(a =>
                                a.Code == expectedLogEntry.Code &&
                                a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path) &&
                                a.LineNumber == expectedLogEntry.StartLine &&
                                a.ColumnNumber == expectedLogEntry.StartColumn &&
                                a.EndLineNumber == expectedLogEntry.EndLine &&
                                a.EndColumnNumber == expectedLogEntry.EndColumn
                                )));
                        break;

                    default:
                        throw new Exception(string.Format("Unexpected IssueKind: {0}", expectedLogEntry.IssueKind));
                }
            }

            var sourceFullPaths = new List<string>();
            foreach (var sourceFileName in sourceFileNames.EmptyIfNull())
            {
                sourceFullPaths.Add(Path.Combine(baseDirecytory, sourceFileName));
            }

            var nsDepCopTask = new NsDepCopTask()
            {
                BaseDirectory = new TestTaskItem(baseDirecytory),
                Compile = sourceFullPaths.Select(i => new TestTaskItem(i)).ToArray(),
                ReferencePath = referencedFilePaths.EmptyIfNull().Select(i => new TestTaskItem(i)).ToArray()
            };
            nsDepCopTask.BuildEngine = mockBuildEngine;

            return nsDepCopTask;
        }

        /// <summary>
        /// Converts a test source file name to full path.
        /// </summary>
        /// <param name="baseDirecytory">The full path of the base directory.</param>
        /// <param name="sourceFileName">The name of the source file.</param>
        /// <returns></returns>
        private static string TestSourceFileNameToFullPath(string baseDirecytory, string sourceFileName)
        {
            return baseDirecytory != null && sourceFileName != null
                ? Path.Combine(baseDirecytory, sourceFileName)
                : null;
        }

    }
}
