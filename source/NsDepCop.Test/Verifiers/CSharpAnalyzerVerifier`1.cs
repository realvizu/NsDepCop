using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Codartis.NsDepCop.Test.Verifiers
{
    public static partial class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
        public static DiagnosticResult Diagnostic()
            => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
        public static DiagnosticResult Diagnostic(string diagnosticId)
            => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
        public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
            => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

        /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
        public static async Task VerifyAnalyzerAsync(string source,
            IEnumerable<DiagnosticResult> expected = null,
            [CallerMemberName] string callerMethodName = null)
        {
            var configFilePath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                "RoslynAnalyzer",
                callerMethodName!,
                "config.nsdepcop");

            var test = new Test(configFilePath)
            {
                TestCode = source,
            };

            if (expected != null)
                test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync(CancellationToken.None);
        }
    }
}