using Codartis.NsDepCop.Analyzer.Factory;
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
            var specification = new TestCaseSpecification("TestFiles_NoConfigFile",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_NO_CONFIG_FILE } });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_NonXmlConfigFile()
        {
            var specification = new TestCaseSpecification("TestFiles_NonXmlConfigFile",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Error, Code = NsDepCopTask.MSBUILD_CODE_EXCEPTION } });

            ExecuteWithAllAnalyzers(specification, expectedReturnValue: false);
        }

        [TestMethod]
        public void Execute_ConfigFileDisabled()
        {
            var specification = new TestCaseSpecification("TestFiles_ConfigDisabled",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Warning, Code = NsDepCopTask.MSBUILD_CODE_CONFIG_DISABLED } });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_ConfigFileErrorsIgnored()
        {
            var specification = new TestCaseSpecification("TestFiles_ConfigFileErrorsIgnored",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Warning, Code = NsDepCopTask.MSBUILD_CODE_CONFIG_DISABLED } });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_AllowedDependency()
        {
            var specification = new TestCaseSpecification("TestFiles_AllowedDependency",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO } },
                new string[] { "AllowedDependency.cs" });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_SameNamespaceAlwaysAllowed()
        {
            var specification = new TestCaseSpecification("TestFiles_SameNamespaceAlwaysAllowed",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO } },
                new string[] { "SameNamespaceAlwaysAllowed.cs" });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportWarning()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_IdentifierName_ReportWarning",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportInfo()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportInfo.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_IdentifierName_ReportInfo",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportError()
        {
            var sourceFileName = "DepViolation_IdentifierName_ReportError.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_IdentifierName_ReportError",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_QualifiedName()
        {
            var sourceFileName = "DepViolation_QualifiedName.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_QualifiedName",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_AliasQualifiedName()
        {
            var sourceFileName = "DepViolation_AliasQualifiedName.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_AliasQualifiedName",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification, skipEndLocationValidation: true);
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationExpression()
        {
            var sourceFileName = "DepViolation_InvocationExpression.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_InvocationExpression",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationWithTypeArg()
        {
            var sourceFileName = "DepViolation_InvocationWithTypeArg.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_InvocationWithTypeArg",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 40,
                        EndLine = 9,
                        EndColumn = 50
                    },
                },
                new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_MemberAccessExpression()
        {
            var sourceFileName = "DepViolation_MemberAccessExpression.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_MemberAccessExpression",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 24,
                        EndLine = 9,
                        EndColumn = 47
                    },
                },
                new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_GenericName()
        {
            var sourceFileName = "DepViolation_GenericName.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_GenericName",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_GenericTypeArgument()
        {
            var sourceFileName = "DepViolation_GenericTypeArgument.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_GenericTypeArgument",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 34,
                        EndLine = 8,
                        EndColumn = 44
                    },                
                },
                new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_NestedType()
        {
            var sourceFileName = "DepViolation_NestedType.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_NestedType",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 36
                    },
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
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 38
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 27
                    },
                },
            new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_ArrayType()
        {
            var sourceFileName = "DepViolation_ArrayType.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_ArrayType",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_NullableType()
        {
            var sourceFileName = "DepViolation_NullableType.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_NullableType",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_DepViolation_EveryUserDefinedTypeKind()
        {
            var sourceFileName = "DepViolation_EveryUserDefinedTypeKind.cs";
            var specification = new TestCaseSpecification("TestFiles_DepViolation_EveryUserDefinedTypeKind",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 24
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 8,
                        StartColumn = 17,
                        EndLine = 8,
                        EndColumn = 29
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 9,
                        StartColumn = 17,
                        EndLine = 9,
                        EndColumn = 25
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 10,
                        StartColumn = 17,
                        EndLine = 10,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFileName,
                        StartLine = 11,
                        StartColumn = 17,
                        EndLine = 11,
                        EndColumn = 27
                    },
                },
                new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_TooManyIssues()
        {
            var sourceFileName = "TooManyIssues.cs";
            var specification = new TestCaseSpecification("TestFiles_TooManyIssues",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
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
                    new LogEntryParameters { IssueKind = IssueKind.Warning, Code = NsDepCopTask.MSBUILD_CODE_TOO_MANY_ISSUES},
                },
                new string[] { sourceFileName });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_NoSourceFiles()
        {
            var specification = new TestCaseSpecification("TestFiles_NoSourceFiles",
                new[] { new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO } });

            ExecuteWithAllAnalyzers(specification);
        }

        [TestMethod]
        public void Execute_MultipleSourceFiles()
        {
            var sourceFile1 = "SourceFile1.cs";
            var sourceFile2 = "SourceFile2.cs";
            var specification = new TestCaseSpecification("TestFiles_MultipleSourceFiles",
                new[] 
                {
                    new LogEntryParameters { IssueKind = IssueKind.Info, Code = NsDepCopTask.MSBUILD_CODE_INFO },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFile1,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                    new LogEntryParameters
                    { 
                        IssueKind = IssueKind.Warning, 
                        Code = NsDepCopTask.MSBUILD_CODE_ISSUE,
                        Path = sourceFile2,
                        StartLine = 7,
                        StartColumn = 17,
                        EndLine = 7,
                        EndColumn = 23
                    },
                },
                new string[] { sourceFile1, sourceFile2 });

            ExecuteWithAllAnalyzers(specification);
        }

        /// <summary>
        /// Creates an NsDepCopTask and sets it up for testing. Adds a mocked BuildEngine and log expectations.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        /// <param name="skipEndLocationValidation">End location validation can be omitted. Needed because of an NRefactory bug.</param>
        /// <returns>A new NsDepCopTask instance ready for testing.</returns>
        private static NsDepCopTask SetUpNsDepCopTaskForTest(
            TestCaseSpecification specification,
            bool skipEndLocationValidation = false)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var baseDirecytory = Path.Combine(assemblyDirectory, specification.TestFilesFolderName);

            var mockBuildEngine = MockRepository.GenerateStrictMock<IBuildEngine>();
            foreach (var expectedLogEntry in specification.ExpectedLogEntries)
            {
                switch (expectedLogEntry.IssueKind)
                {
                    case (IssueKind.Info):
                        mockBuildEngine
                            .Expect(i => i.LogMessageEvent(Arg<BuildMessageEventArgs>.Matches(a =>
                                a.Code == expectedLogEntry.Code
                                && a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path)
                                && a.LineNumber == expectedLogEntry.StartLine
                                && a.ColumnNumber == expectedLogEntry.StartColumn
                                && (skipEndLocationValidation || a.EndLineNumber == expectedLogEntry.EndLine)
                                && (skipEndLocationValidation || a.EndColumnNumber == expectedLogEntry.EndColumn)
                                ))).Repeat.Once();
                        break;

                    case (IssueKind.Warning):
                        mockBuildEngine
                            .Expect(i => i.LogWarningEvent(Arg<BuildWarningEventArgs>.Matches(a =>
                                a.Code == expectedLogEntry.Code
                                && a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path)
                                && a.LineNumber == expectedLogEntry.StartLine
                                && a.ColumnNumber == expectedLogEntry.StartColumn
                                && (skipEndLocationValidation || a.EndLineNumber == expectedLogEntry.EndLine)
                                && (skipEndLocationValidation || a.EndColumnNumber == expectedLogEntry.EndColumn)
                                ))).Repeat.Once();
                        break;

                    case (IssueKind.Error):
                        mockBuildEngine
                            .Expect(i => i.LogErrorEvent(Arg<BuildErrorEventArgs>.Matches(a =>
                                a.Code == expectedLogEntry.Code
                                && a.File == TestSourceFileNameToFullPath(baseDirecytory, expectedLogEntry.Path)
                                && a.LineNumber == expectedLogEntry.StartLine
                                && a.ColumnNumber == expectedLogEntry.StartColumn
                                && (skipEndLocationValidation || a.EndLineNumber == expectedLogEntry.EndLine)
                                && (skipEndLocationValidation || a.EndColumnNumber == expectedLogEntry.EndColumn)
                                ))).Repeat.Once();
                        break;

                    default:
                        throw new Exception(string.Format("Unexpected IssueKind: {0}", expectedLogEntry.IssueKind));
                }
            }

            var sourceFullPaths = new List<string>();
            foreach (var sourceFileName in specification.SourceFileNames.EmptyIfNull())
            {
                sourceFullPaths.Add(Path.Combine(baseDirecytory, sourceFileName));
            }

            var nsDepCopTask = new NsDepCopTask()
            {
                BaseDirectory = new TestTaskItem(baseDirecytory),
                Compile = sourceFullPaths.Select(i => new TestTaskItem(i)).ToArray(),
                ReferencePath = specification.ReferencedFilePaths.EmptyIfNull().Select(i => new TestTaskItem(i)).ToArray(),
            };
            nsDepCopTask.BuildEngine = mockBuildEngine;

            return nsDepCopTask;
        }

        /// <summary>
        /// Executes the test case using both analyzers.
        /// </summary>
        /// <param name="specification">The test case specification.</param>
        /// <param name="expectedReturnValue">The expected return value of the Task.Execute function.</param>
        /// <param name="skipEndLocationValidation">End location validation can be omitted. Needed because of an NRefactory bug.</param>
        private static void ExecuteWithAllAnalyzers(
            TestCaseSpecification specification, 
            bool expectedReturnValue = true,
            bool skipEndLocationValidation = false)
        {
            {
                var nsDepCopTask = SetUpNsDepCopTaskForTest(specification, skipEndLocationValidation);
                nsDepCopTask.Parser = new TestTaskItem(Parser.NRefactory.ToString());
                nsDepCopTask.Execute().ShouldEqual(expectedReturnValue);
                nsDepCopTask.BuildEngine.VerifyAllExpectations();
            }
            {
                var nsDepCopTask = SetUpNsDepCopTaskForTest(specification);
                nsDepCopTask.Parser = new TestTaskItem(Parser.Roslyn.ToString());
                nsDepCopTask.Execute().ShouldEqual(expectedReturnValue);
                nsDepCopTask.BuildEngine.VerifyAllExpectations();
            }
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
