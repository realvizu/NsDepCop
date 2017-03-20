using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Unit tests for the NsDepCopTask class.
    /// </summary>
    [TestClass]
    public class NsDepCopTaskTests : MockedNsDepCopTaskTestBase
    {
        [TestMethod]
        public void Execute_NoConfigFile()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_NoConfigFile",
                ExpectedLogEntries = new[]
                {
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.NoConfigFileIssue)
                },
                ExpectStartEvent = false,
                ExpectEndEvent = false,
            });
        }

        [TestMethod]
        public void Execute_NonXmlConfigFile()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_NonXmlConfigFile",
                ExpectedLogEntries = new[]
                {
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigExceptionIssue)
                },
                ExpectedReturnValue = false,
                ExpectStartEvent = false,
                ExpectEndEvent = false,
            });
        }

        [TestMethod]
        public void Execute_ConfigFileErrorCausesException()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ConfigFileErrorCausesException",
                ExpectedLogEntries = new[]
                {
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigExceptionIssue)
                },
                ExpectedReturnValue = false,
                ExpectStartEvent = false,
                ExpectEndEvent = false,
            });
        }

        [TestMethod]
        public void Execute_ConfigFileDisabled()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ConfigDisabled",
                ExpectedLogEntries = new[]
                {
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.ConfigDisabledIssue)
                },
                ExpectStartEvent = false,
                ExpectEndEvent = false
            });
        }

        [TestMethod]
        public void Execute_ConfigFileExtraElementsIgnored()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ConfigFileExtraElementsIgnored",
            });
        }

        [TestMethod]
        public void Execute_ExceptionCaughtAndReported()
        {
            var expectedLogEntries = new[]
            {
                LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskExceptionIssue)
            };

            var mockBuildEngine = MockRepository.GenerateStrictMock<IBuildEngine>();
            ExpectEvents(mockBuildEngine, expectedLogEntries);

            var nsDepCopTask = new NsDepCopTask
            {
                BuildEngine = mockBuildEngine,
                BaseDirectory = null,
                Compile = null,
                ReferencePath = null,
            };
            nsDepCopTask.Execute();
        }

        [TestMethod]
        public void Execute_ConfigInfoImportanceIsHigh()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ConfigInfoImportanceIsHigh",
                SourceFileNames = new[] { "ConfigInfoImportanceIsHigh.cs" },
                ExpectStartEvent = false,
                ExpectEndEvent = false,
                ExpectedLogEntries = new[]
                {
                    LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskStartedIssue, Importance.High),
                    LogEntryParameters.FromIssueDescriptor(NsDepCopTask.TaskFinishedIssue, Importance.High),
                }
            });
        }

        [TestMethod]
        public void Execute_AllowedDependency()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_AllowedDependency",
                SourceFileNames = new[] { "AllowedDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_SameNamespaceAlwaysAllowed()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_SameNamespaceAlwaysAllowed",
                SourceFileNames = new[] { "SameNamespaceAlwaysAllowed.cs" },
            });
        }

        [TestMethod]
        public void Execute_SameNamespaceAllowedEvenWhenVisibleMembersDefined()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_SameNamespaceAllowedEvenWhenVisibleMembersDefined",
                SourceFileNames = new[] { "SameNamespaceAllowedEvenWhenVisibleMembersDefined.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportWarning()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportWarning",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportInfo()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportInfo.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportInfo",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23, IssueKind.Info)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportError()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportError.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportError",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23, IssueKind.Error)
                },
                ExpectedReturnValue = false
            });
        }

        [TestMethod]
        public void Execute_DepViolation_QualifiedName()
        {
            const string sourceFileName = "DepViolation_QualifiedName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_QualifiedName",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 5, 19, 5, 25)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_AliasQualifiedName()
        {
            const string sourceFileName = "DepViolation_AliasQualifiedName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_AliasQualifiedName",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 25, 7, 31)
                },
                SkipLocationValidation = true
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationExpression()
        {
            const string sourceFileName = "DepViolation_InvocationExpression.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationExpression",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 10, 13, 10, 26),
                    CreateLogEntryParameters(sourceFileName, 11, 26, 11, 45),
                    CreateLogEntryParameters(sourceFileName, 12, 32, 12, 45),
                    CreateLogEntryParameters(sourceFileName, 15, 9, 15, 15)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationWithTypeArg()
        {
            const string sourceFileName = "DepViolation_InvocationWithTypeArg.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationWithTypeArg",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 9, 42, 9, 50)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_MemberAccessExpression()
        {
            const string sourceFileName = "DepViolation_MemberAccessExpression.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_MemberAccessExpression",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 9, 37, 9, 47)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_GenericName()
        {
            const string sourceFileName = "DepViolation_GenericName.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericName",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 31),
                    CreateLogEntryParameters(sourceFileName, 11, 17, 11, 31),
                    CreateLogEntryParameters(sourceFileName, 11, 32, 11, 40)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_GenericTypeArgument()
        {
            const string sourceFileName = "DepViolation_GenericTypeArgument.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericTypeArgument",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 32, 7, 40),
                    CreateLogEntryParameters(sourceFileName, 8, 36, 8, 44)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_NestedType()
        {
            const string sourceFileName = "DepViolation_NestedType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_NestedType",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 25),
                    CreateLogEntryParameters(sourceFileName, 7, 26, 7, 36),
                    CreateLogEntryParameters(sourceFileName, 8, 19, 8, 27),
                    CreateLogEntryParameters(sourceFileName, 8, 28, 8, 38)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ArrayType()
        {
            const string sourceFileName = "DepViolation_ArrayType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ArrayType",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 25)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_NullableType()
        {
            const string sourceFileName = "DepViolation_NullableType.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_NullableType",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 25)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_EveryUserDefinedTypeKind()
        {
            const string sourceFileName = "DepViolation_EveryUserDefinedTypeKind.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_EveryUserDefinedTypeKind",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 24),
                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 29),
                    CreateLogEntryParameters(sourceFileName, 9, 17, 9, 25),
                    CreateLogEntryParameters(sourceFileName, 10, 17, 10, 23),
                    CreateLogEntryParameters(sourceFileName, 11, 17, 11, 27)
                },
            });
        }

        [TestMethod]
        public void Execute_TooManyIssues()
        {
            const string sourceFileName = "TooManyIssues.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_TooManyIssues",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23),
                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 23),
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.TooManyIssuesIssue),
                },
            });
        }

        [TestMethod]
        public void Execute_MaxIssueCountEqualsIssueCount()
        {
            const string sourceFileName = "MaxIssueCountEqualsIssueCount.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_MaxIssueCountEqualsIssueCount",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 17, 7, 23),
                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 23),
                    LogEntryParameters.FromIssueDescriptor(IssueDefinitions.TooManyIssuesIssue),
                },
            });
        }

        [TestMethod]
        public void Execute_NoSourceFiles()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_NoSourceFiles",
            });
        }

        [TestMethod]
        public void Execute_MultipleSourceFiles()
        {
            const string sourceFile1 = "SourceFile1.cs";
            const string sourceFile2 = "SourceFile2.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_MultipleSourceFiles",
                SourceFileNames = new[] {sourceFile1, sourceFile2},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFile1, 7, 17, 7, 23),
                    CreateLogEntryParameters(sourceFile2, 7, 17, 7, 23),
                },
            });
        }

        [TestMethod]
        public void Execute_AllowedDependency_ByChildCanDependOnParentImplicitlyOption()
        {
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ByChildCanDependOnParentImplicitlyOption",
                SourceFileNames = new[] { "ChildToParentDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ExtensionMethodInvocation()
        {
            const string sourceFileName = "DepViolation_ExtensionMethodInvocation.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ExtensionMethodInvocation",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 9, 27, 9, 44),
                    CreateLogEntryParameters(sourceFileName, 10, 27, 10, 51)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ObjectCreationExpression()
        {
            const string sourceFileName = "DepViolation_ObjectCreationExpression.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ObjectCreationExpression",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 9, 17, 9, 29)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_AllowedDependencyInvisibleMembers()
        {
            const string sourceFileName = "AllowedDependencyInvisibleMembers.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_AllowedDependencyInvisibleMembers",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 6, 19, 6, 32),
                    CreateLogEntryParameters(sourceFileName, 8, 19, 8, 43),
                    CreateLogEntryParameters(sourceFileName, 9, 19, 9, 39)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_Var()
        {
            const string sourceFileName = "var.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_Var",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 13, 7, 16),
                    CreateLogEntryParameters(sourceFileName, 7, 23, 7, 29),
                    CreateLogEntryParameters(sourceFileName, 7, 30, 7, 40)
                },
            });
        }

        // TODO
        [TestMethod, Ignore]
        public void Execute_DepViolation_VarWithConstructedGenericType()
        {
            const string sourceFileName = "var.cs";
            ExecuteWithAllAnalyzers(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_VarWithConstructedGenericType",
                SourceFileNames = new[] {sourceFileName},
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 8, 13, 8, 16),
                    CreateLogEntryParameters(sourceFileName, 8, 13, 8, 16),
                    CreateLogEntryParameters(sourceFileName, 8, 23, 8, 30),
                    CreateLogEntryParameters(sourceFileName, 8, 33, 8, 39),
                    CreateLogEntryParameters(sourceFileName, 8, 41, 8, 49),
                    CreateLogEntryParameters(sourceFileName, 8, 41, 8, 49),
                },
            });
        }
    }
}
