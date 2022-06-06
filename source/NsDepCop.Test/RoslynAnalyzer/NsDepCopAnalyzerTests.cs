using System;
using System.Threading.Tasks;
using Codartis.NsDepCop.RoslynAnalyzer;
using Codartis.NsDepCop.Test.Verifiers;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Codartis.NsDepCop.Test.RoslynAnalyzer
{
    public class NsDepCopAnalyzerTests
    {
        public NsDepCopAnalyzerTests()
        {
            Environment.SetEnvironmentVariable(ProductConstants.DisableToolEnvironmentVariableName, null);
        }

        [Fact]
        public async Task EmptySource_NoIssues()
        {
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(string.Empty);
        }

        [Fact]
        public async Task ToolDisabled_ReturnsIssue()
        {
            Environment.SetEnvironmentVariable(ProductConstants.DisableToolEnvironmentVariableName, "1");


            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.ToolDisabled).WithLocation(1, 1)
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(string.Empty, expectation);
        }

        [Fact]
        public async Task NoConfigFile_ReturnsIssue()
        {
            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.NoConfigFile).WithLocation(1, 1)
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(string.Empty, expectation);
        }

        [Fact]
        public async Task ConfigException_ReturnsIssue()
        {
            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.ConfigException)
                    .WithArguments("Unexpected end of file has occurred. The following elements are not closed: NsDepCopConfig. Line 1, position 17.")
                    .WithLocation(1, 1)
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(string.Empty, expectation);
        }

        [Fact]
        public async Task ConfigDisabled_ReturnsIssue()
        {
            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.ConfigDisabled).WithLocation(1, 1)
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(string.Empty, expectation);
        }

        [Fact]
        public async Task SourceWithDependencyProblems_ReturnsIssues()
        {
            const string source = @"
namespace A
{
    class C1 
    {
        B.C2 _f1;
    }
}

namespace B
{
    class C2
    {
        A.C1 _f2;
    }
}
";

            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.IllegalDependency)
                    .WithArguments("A", "B", "C1", "C2")
                    .WithLocation(6, 11),
                new DiagnosticResult(DiagnosticDefinitions.IllegalDependency)
                    .WithArguments("B", "A", "C2", "C1")
                    .WithLocation(14, 11),
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(source, expectation);
        }

        [Fact(Skip = "The analyzer works on multiple threads concurrently and it's not deterministic" +
                     " which 2 of the 5 invalid dependencies get reported." +
                     " I don't know how to test it in a deterministic way.")]
        public async Task TooManyDependencyIssues_StopsAnalysisWithWarning()
        {
            const string source = @"
namespace A
{
    class C1 
    {
        B.C2 _f1;
        B.C2 _f2;
        B.C2 _f3;
        B.C2 _f4;
        B.C2 _f5;
    }
}

namespace B
{
    class C2 {}
}
";

            var expectation = new[]
            {
                new DiagnosticResult(DiagnosticDefinitions.IllegalDependency)
                    .WithArguments("A", "B", "C1", "C2").WithNoLocation()
                    .WithLocation(6, 11),
                new DiagnosticResult(DiagnosticDefinitions.IllegalDependency)
                    .WithArguments("A", "B", "C1", "C2").WithNoLocation()
                    .WithLocation(7, 11),
                new DiagnosticResult(DiagnosticDefinitions.TooManyDependencyIssues)
                    .WithArguments("2")
                    .WithLocation(8, 11),
            };
            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(source, expectation);
        }
    }
}