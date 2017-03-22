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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_AllowedDependency",
                SourceFileNames = new[] { "AllowedDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_SameNamespaceAlwaysAllowed()
        {
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_SameNamespaceAlwaysAllowed",
                SourceFileNames = new[] { "SameNamespaceAlwaysAllowed.cs" },
            });
        }

        [TestMethod]
        public void Execute_SameNamespaceAllowedEvenWhenVisibleMembersDefined()
        {
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_SameNamespaceAllowedEvenWhenVisibleMembersDefined",
                SourceFileNames = new[] { "SameNamespaceAllowedEvenWhenVisibleMembersDefined.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_IdentifierName_ReportWarning()
        {
            const string sourceFileName = "DepViolation_IdentifierName_ReportWarning.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_IdentifierName_ReportWarning",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_QualifiedName",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_AliasQualifiedName",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 25, 7, 31)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationExpression()
        {
            const string sourceFileName = "DepViolation_InvocationExpression.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationExpression",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // Class3
                    CreateLogEntryParameters(sourceFileName, 9, 13, 9, 20),
                    // Class3
                    CreateLogEntryParameters(sourceFileName, 10, 26, 10, 33),
                    // Class3
                    CreateLogEntryParameters(sourceFileName, 11, 20, 11, 27),
                    // Class4<Class3>
                    CreateLogEntryParameters(sourceFileName, 12, 20, 12, 27),
                    CreateLogEntryParameters(sourceFileName, 12, 20, 12, 27),
                    // Class3
                    CreateLogEntryParameters(sourceFileName, 15, 11, 15, 17)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_InvocationWithTypeArg()
        {
            const string sourceFileName = "DepViolation_InvocationWithTypeArg.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_InvocationWithTypeArg",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // Class3
                    CreateLogEntryParameters(sourceFileName, 10, 28, 10, 34),
                    // Class4<Class3>
                    CreateLogEntryParameters(sourceFileName, 11, 28, 11, 42),
                    CreateLogEntryParameters(sourceFileName, 11, 35, 11, 41),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_MemberAccessExpression()
        {
            const string sourceFileName = "DepViolation_MemberAccessExpression.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_MemberAccessExpression",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericName",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // MyGenericClass<MyClass2>
                    CreateLogEntryParameters(sourceFileName, 8, 17, 8, 41),

                    // MyGenericClass2<MyClass3, MyClass2, MyClass3>
                    CreateLogEntryParameters(sourceFileName, 11, 17, 11, 62),
                    // MyClass3
                    CreateLogEntryParameters(sourceFileName, 11, 33, 11, 41),
                    // MyClass3
                    CreateLogEntryParameters(sourceFileName, 11, 53, 11, 61),

                    // MyGenericClass<MyGenericClass<MyClass3>>
                    CreateLogEntryParameters(sourceFileName, 14, 17, 14, 57),
                    // MyGenericClass<MyClass3>
                    CreateLogEntryParameters(sourceFileName, 14, 32, 14, 56),
                    // MyClass3
                    CreateLogEntryParameters(sourceFileName, 14, 47, 14, 55),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_GenericTypeArgument()
        {
            const string sourceFileName = "DepViolation_GenericTypeArgument.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_GenericTypeArgument",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_NestedType",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ArrayType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 19, 7, 25),
                    CreateLogEntryParameters(sourceFileName, 11, 20, 11, 27),
                    CreateLogEntryParameters(sourceFileName, 12, 20, 12, 27),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_PointerType()
        {
            const string sourceFileName = "DepViolation_PointerType.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_PointerType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 19, 7, 25),
                    CreateLogEntryParameters(sourceFileName, 11, 20, 11, 27),
                    CreateLogEntryParameters(sourceFileName, 12, 20, 12, 27),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_VeryComplexType()
        {
            const string sourceFileName = "DepViolation_VeryComplexType.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_VeryComplexType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // Class4<Class3[], Class4<Class3*[], Class3[][]>>
                    CreateLogEntryParameters(sourceFileName, 10, 13, 10, 60),
                    CreateLogEntryParameters(sourceFileName, 10, 20, 10, 26),
                    CreateLogEntryParameters(sourceFileName, 10, 30, 10, 59),
                    CreateLogEntryParameters(sourceFileName, 10, 37, 10, 43),
                    CreateLogEntryParameters(sourceFileName, 10, 48, 10, 54),
                    // Method2 return value
                    CreateLogEntryParameters(sourceFileName, 10, 72, 10, 79),
                    CreateLogEntryParameters(sourceFileName, 10, 72, 10, 79),
                    CreateLogEntryParameters(sourceFileName, 10, 72, 10, 79),
                    CreateLogEntryParameters(sourceFileName, 10, 72, 10, 79),
                    CreateLogEntryParameters(sourceFileName, 10, 72, 10, 79),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_NullableType()
        {
            const string sourceFileName = "DepViolation_NullableType.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_NullableType",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_EveryUserDefinedTypeKind",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_TooManyIssues",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_MaxIssueCountEqualsIssueCount",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_NoSourceFiles",
            });
        }

        [TestMethod]
        public void Execute_MultipleSourceFiles()
        {
            const string sourceFile1 = "SourceFile1.cs";
            const string sourceFile2 = "SourceFile2.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_MultipleSourceFiles",
                SourceFileNames = new[] { sourceFile1, sourceFile2 },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_ByChildCanDependOnParentImplicitlyOption",
                SourceFileNames = new[] { "ChildToParentDependency.cs" },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_ExtensionMethodInvocation()
        {
            const string sourceFileName = "DepViolation_ExtensionMethodInvocation.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ExtensionMethodInvocation",
                SourceFileNames = new[] { sourceFileName },
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
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_ObjectCreationExpression",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 9, 17, 9, 29)
                },
            });
        }

        [TestMethod]
        public void Execute_AllowedDependencyWithInvisibleMembers()
        {
            const string sourceFileName = "AllowedDependencyWithInvisibleMembers.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_AllowedDependencyWithInvisibleMembers",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 6, 19, 6, 32),
                    CreateLogEntryParameters(sourceFileName, 8, 19, 8, 43),
                    CreateLogEntryParameters(sourceFileName, 9, 19, 9, 47)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_Var()
        {
            const string sourceFileName = "DepViolation_Var.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_Var",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    CreateLogEntryParameters(sourceFileName, 7, 13, 7, 16),
                    CreateLogEntryParameters(sourceFileName, 7, 23, 7, 29),
                    CreateLogEntryParameters(sourceFileName, 7, 30, 7, 40)
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_VarWithConstructedGenericType()
        {
            const string sourceFileName = "DepViolation_VarWithConstructedGenericType.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_VarWithConstructedGenericType",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // var: ClassB`2, EnumB, EnumB
                    CreateLogEntryParameters(sourceFileName, 8, 13, 8, 16),
                    CreateLogEntryParameters(sourceFileName, 8, 13, 8, 16),
                    CreateLogEntryParameters(sourceFileName, 8, 13, 8, 16),

                    // ClassB<B.EnumB, EnumA, B.EnumB>
                    CreateLogEntryParameters(sourceFileName, 8, 23, 8, 54),
                    // EnumB
                    CreateLogEntryParameters(sourceFileName, 8, 32, 8, 37),
                    // EnumB
                    CreateLogEntryParameters(sourceFileName, 8, 48, 8, 53),

                    // Instance: ClassB`2, EnumB, EnumB
                    CreateLogEntryParameters(sourceFileName, 8, 55, 8, 63),
                    CreateLogEntryParameters(sourceFileName, 8, 55, 8, 63),
                    CreateLogEntryParameters(sourceFileName, 8, 55, 8, 63),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_Attributes()
        {
            const string sourceFileName = "DepViolation_Attributes.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_Attributes",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // [Forbidden(...
                    CreateLogEntryParameters(sourceFileName, 15, 6, 15, 15),

                    // class attribute type parameter
                    CreateLogEntryParameters(sourceFileName, 21, 41, 21, 54),

                    // field attribute type parameter
                    CreateLogEntryParameters(sourceFileName, 29, 45, 29, 58),

                    // enum value attribute type parameter
                    CreateLogEntryParameters(sourceFileName, 38, 45, 38, 58),
                },
            });
        }

        [TestMethod]
        public void Execute_DepViolation_Delegates()
        {
            const string sourceFileName = "DepViolation_Delegates.cs";
            ExecuteTest(new TestCaseSpecification
            {
                TestFilesFolderName = "TestFiles_DepViolation_Delegates",
                SourceFileNames = new[] { sourceFileName },
                ExpectedLogEntries = new[]
                {
                    // delegate Class1<Class2> Delegate1(Class2 c);
                    CreateLogEntryParameters(sourceFileName, 5, 14, 5, 28),
                    CreateLogEntryParameters(sourceFileName, 5, 21, 5, 27),
                    CreateLogEntryParameters(sourceFileName, 5, 39, 5, 45),
                },
            });
        }
    }
}
