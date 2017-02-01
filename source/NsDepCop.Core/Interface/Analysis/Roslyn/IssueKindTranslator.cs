using System;
using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Roslyn
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
                    throw new InvalidCastException(
                        $"Cannot convert {diagnosticSeverity} to Codartis.NsDepCop.Core.IssueKind.");
            }
        }

        /// <summary>
        /// Translates from Codartis.NsDepCop.Core.IssueKind to Microsoft.CodeAnalysis.DiagnosticSeverity.
        /// </summary>
        /// <param name="issueKind">A Codartis.NsDepCop.Core.IssueKind value.</param>
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
                    throw new InvalidCastException(
                        $"Cannot convert {issueKind} to Microsoft.CodeAnalysis.DiagnosticSeverity.");
            }
        }
    }
}
