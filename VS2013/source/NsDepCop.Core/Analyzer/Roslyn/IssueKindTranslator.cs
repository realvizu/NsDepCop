using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;
using System;

namespace Codartis.NsDepCop.Core.Analyzer.Roslyn
{
    /// <summary>
    /// Translates between Microsoft.CodeAnalysis.DiagnosticSeverity and Codartis.NsDepCop.Core.IssueKind.
    /// </summary>
    internal static class IssueKindTranslator
    {
        /// <summary>
        /// Translates from Microsoft.CodeAnalysis.DiagnosticSeverity to Codartis.NsDepCop.Core.IssueKind.
        /// </summary>
        /// <param name="diagnosticSeverity">A Microsoft.CodeAnalysis.DiagnosticSeverity value.</param>
        /// <returns>A Codartis.NsDepCop.Core.IssueKind value.</returns>
        public static IssueKind ToIssueKind(DiagnosticSeverity diagnosticSeverity)
        {
            switch(diagnosticSeverity)
            {
                case(DiagnosticSeverity.Error):
                    return IssueKind.Error;
                case(DiagnosticSeverity.Info):
                    return IssueKind.Info;
                case (DiagnosticSeverity.None):
                    return IssueKind.None;
                case (DiagnosticSeverity.Warning):
                    return IssueKind.Warning;
                default:
                    throw new InvalidCastException(
                        string.Format("Cannot convert {0} to Codartis.NsDepCop.Core.IssueKind.", diagnosticSeverity.ToString()));
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
                case (IssueKind.Error):
                    return DiagnosticSeverity.Error;
                case (IssueKind.Info):
                    return DiagnosticSeverity.Info;
                case (IssueKind.None):
                    return DiagnosticSeverity.None;
                case (IssueKind.Warning):
                    return DiagnosticSeverity.Warning;
                default:
                    throw new InvalidCastException(
                        string.Format("Cannot convert {0} to Microsoft.CodeAnalysis.DiagnosticSeverity.", issueKind.ToString()));
            }
        }
    }
}
