using Codartis.NsDepCop.ParserAdapter.Roslyn1x;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// This diagnostic analyzer is invoked by Visual Studio/Roslyn and it reports namespace dependency issues to the VS IDE.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NsDepCopDiagnosticAnalyzer : NsDepCopDiagnosticAnalyzerBase
    {
        public NsDepCopDiagnosticAnalyzer() 
            : base (new Roslyn1TypeDependencyEnumerator(LogTraceMessage))
        {
        }
   }
}