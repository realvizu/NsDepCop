using System;
using Codartis.NsDepCop.Interface.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    internal static class IssueKindTranslator
    {
        /// <summary>
        /// Translates from Microsoft.CodeAnalysis.DiagnosticSeverity to IssueKind.
        /// </summary>
        /// <param name="diagnosticSeverity">A Microsoft.CodeAnalysis.DiagnosticSeverity value.</param>
        /// <returns>An IssueKind value.</returns>
        public static IssueKind ToIssueKind(DiagnosticSeverity diagnosticSeverity)
        {
            switch(diagnosticSeverity)
            {
                case DiagnosticSeverity.Error:
                    return IssueKind.Error;
                case DiagnosticSeverity.Info:
                    return IssueKind.Info;
                case DiagnosticSeverity.Hidden:
                    return IssueKind.Hidden;
                case DiagnosticSeverity.Warning:
                    return IssueKind.Warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(diagnosticSeverity), diagnosticSeverity, "Unexpected value.");
            }
        }

        /// <summary>
        /// Translates from Codartis.NsDepCop.Core.IssueKind to Microsoft.CodeAnalysis.DiagnosticSeverity.
        /// </summary>
        /// <param name="issueKind">An IssueKind value.</param>
        /// <returns>A Microsoft.CodeAnalysis.DiagnosticSeverity value.</returns>
        public static DiagnosticSeverity ToDiagnosticSeverity(IssueKind issueKind)
        {
            switch (issueKind)
            {
                case IssueKind.Error:
                    return DiagnosticSeverity.Error;
                case IssueKind.Info:
                    return DiagnosticSeverity.Info;
                case IssueKind.Hidden:
                    return DiagnosticSeverity.Hidden;
                case IssueKind.Warning:
                    return DiagnosticSeverity.Warning;
                default:
                    throw new ArgumentOutOfRangeException(nameof(issueKind), issueKind, "Unexpected value.");
            }
        }
    }
}
