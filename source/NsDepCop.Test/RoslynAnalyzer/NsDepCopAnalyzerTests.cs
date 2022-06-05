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
    }
}