using Codartis.NsDepCop.Interface.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    public static class IssueKindExtension
    {
        /// <summary>
        /// Translates from IssueKind to Microsoft.CodeAnalysis.DiagnosticSeverity.
        /// </summary>
        /// <param name="issueKind">An IssueKind value.</param>
        /// <returns>A Microsoft.CodeAnalysis.DiagnosticSeverity value.</returns>
        public static DiagnosticSeverity ToDiagnosticSeverity(this IssueKind issueKind)
        {
            return IssueKindTranslator.ToDiagnosticSeverity(issueKind);
        }
    }
}
