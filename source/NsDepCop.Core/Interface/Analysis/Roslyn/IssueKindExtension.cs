using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Roslyn
{
    public static class IssueKindExtension
    {
        /// <summary>
        /// Translates from IssueKind to Microsoft.CodeAnalysis.DiagnosticSeverity.
        /// </summary>
        /// <param name="issueKind">A Codartis.NsDepCop.Core.IssueKind value.</param>
        /// <returns>A Microsoft.CodeAnalysis.DiagnosticSeverity value.</returns>
        public static DiagnosticSeverity ToDiagnosticSeverity(this IssueKind issueKind)
        {
            return IssueKindTranslator.ToDiagnosticSeverity(issueKind);
        }
    }
}
