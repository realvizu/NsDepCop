//using Codartis.NsDepCop.Core.Interface.Analysis;
//using Codartis.NsDepCop.Core.Interface.Config;
//using Xunit;

//namespace Codartis.NsDepCop.MsBuildTask.Test
//{
//    /// <summary>
//    /// Unit tests for the NsDepCopTask class.
//    /// </summary>
//    public class NsDepCopTaskTests : MockedNsDepCopTaskTestBase
//    {
//        [Fact]
//        public void Execute_NoConfigFile()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_NoConfigFile",
//                ExpectedLogEntries = new[]
//                {
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.NoConfigFileIssue)
//                },
//                ExpectStartEvent = false,
//                ExpectEndEvent = false,
//            });
//        }

//        [Fact]
//        public void Execute_NonXmlConfigFile()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_NonXmlConfigFile",
//                ExpectedLogEntries = new[]
//                {
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigExceptionIssue)
//                },
//                ExpectedReturnValue = false,
//                ExpectStartEvent = false,
//                ExpectEndEvent = false,
//            });
//        }

//        [Fact]
//        public void Execute_ConfigFileErrorCausesException()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_ConfigFileErrorCausesException",
//                ExpectedLogEntries = new[]
//                {
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigExceptionIssue)
//                },
//                ExpectedReturnValue = false,
//                ExpectStartEvent = false,
//                ExpectEndEvent = false,
//            });
//        }

//        [Fact]
//        public void Execute_ConfigFileDisabled()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_ConfigDisabled",
//                ExpectedLogEntries = new[]
//                {
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigDisabledIssue)
//                },
//                ExpectStartEvent = false,
//                ExpectEndEvent = false
//            });
//        }

//        [Fact]
//        public void Execute_ConfigFileExtraElementsIgnored()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_ConfigFileExtraElementsIgnored",
//            });
//        }

//        [Fact]
//        public void Execute_ExceptionCaughtAndReported()
//        {
//            var expectedLogEntries = new[]
//            {
//                LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskExceptionIssue)
//            };

//            ExpectEvents(expectedLogEntries);

//            var nsDepCopTask = new NsDepCopTask
//            {
//                BuildEngine = BuildEngineMock.Object,
//                BaseDirectory = null,
//                Compile = null,
//                ReferencePath = null,
//            };
//            nsDepCopTask.Execute();
//        }

//        [Fact]
//        public void Execute_ConfigInfoImportanceIsHigh()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_ConfigInfoImportanceIsHigh",
//                SourceFileNames = new[] { "ConfigInfoImportanceIsHigh.cs" },
//                ExpectStartEvent = false,
//                ExpectEndEvent = false,
//                ExpectedLogEntries = new[]
//                {
//                    LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskStartedIssue, Importance.High),
//                    LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskFinishedIssue, Importance.High),
//                }
//            });
//        }

//        [Fact]
//        public void Execute_DepViolation_IdentifierName_ReportWarning()
//        {
//            const string sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportWarning",
//                SourceFileNames = new[] { sourceFileName },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23)
//                },
//            });
//        }

//        [Fact]
//        public void Execute_DepViolation_IdentifierName_ReportInfo()
//        {
//            const string sourceFileName = "DepViolation_IdentifierName_ReportInfo.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportInfo",
//                SourceFileNames = new[] { sourceFileName },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23, IssueKind.Info)
//                },
//            });
//        }

//        [Fact]
//        public void Execute_DepViolation_IdentifierName_ReportError()
//        {
//            const string sourceFileName = "DepViolation_IdentifierName_ReportError.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportError",
//                SourceFileNames = new[] { sourceFileName },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23, IssueKind.Error)
//                },
//                ExpectedReturnValue = false
//            });
//        }

//        [Fact]
//        public void Execute_TooManyIssues()
//        {
//            const string sourceFileName = "TooManyIssues.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_TooManyIssues",
//                SourceFileNames = new[] { sourceFileName },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23),
//                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 23),
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.TooManyIssuesIssue),
//                },
//            });
//        }

//        [Fact]
//        public void Execute_MaxIssueCountEqualsIssueCount()
//        {
//            const string sourceFileName = "MaxIssueCountEqualsIssueCount.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_MaxIssueCountEqualsIssueCount",
//                SourceFileNames = new[] { sourceFileName },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23),
//                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 23),
//                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.TooManyIssuesIssue),
//                },
//            });
//        }

//        [Fact]
//        public void Execute_NoSourceFiles()
//        {
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_NoSourceFiles",
//            });
//        }

//        [Fact]
//        public void Execute_MultipleSourceFiles()
//        {
//            const string sourceFile1 = "SourceFile1.cs";
//            const string sourceFile2 = "SourceFile2.cs";
//            ExecuteTest(new TestCaseSpecification
//            {
//                TestFilesFolderName = "TestFiles_MultipleSourceFiles",
//                SourceFileNames = new[] { sourceFile1, sourceFile2 },
//                ExpectedLogEntries = new[]
//                {
//                    CreateLogEntryParameters(sourceFile1, 7, 17, 7, 23),
//                    CreateLogEntryParameters(sourceFile2, 7, 17, 7, 23),
//                },
//            });
//        }
//    }
//}
