using Codartis.NsDepCop.Core;
using Roslyn.Services;
using System;

namespace Codartis.NsDepCop.Analyzer.Roslyn
{
    /// <summary>
    /// Translates between Roslyn.Services.CodeIssueKind and Codartis.NsDepCop.Core.IssueKind.
    /// </summary>
    internal static class IssueKindTranslator
    {
        /// <summary>
        /// Translates from Roslyn.Services.CodeIssueKind to Codartis.NsDepCop.Core.IssueKind.
        /// </summary>
        /// <param name="codeIssueKind">A Roslyn.Services.CodeIssueKind value.</param>
        /// <returns>A Codartis.NsDepCop.Core.IssueKind value.</returns>
        public static IssueKind ToIssueKind(CodeIssueKind codeIssueKind)
        {
            switch(codeIssueKind)
            {
                case(CodeIssueKind.Error):
                    return IssueKind.Error;
                case(CodeIssueKind.Info):
                    return IssueKind.Info;
                case (CodeIssueKind.Unnecessary):
                    return IssueKind.Unnecessary;
                case (CodeIssueKind.Warning):
                    return IssueKind.Warning;
                default:
                    throw new InvalidCastException(
                        string.Format("Cannot convert {0} to Codartis.NsDepCop.Core.IssueKind.", codeIssueKind.ToString()));
            }
        }

        /// <summary>
        /// Translates from Codartis.NsDepCop.Core.IssueKind to Roslyn.Services.CodeIssueKind.
        /// </summary>
        /// <param name="issueKind">A Codartis.NsDepCop.Core.IssueKind value.</param>
        /// <returns>A Roslyn.Services.CodeIssueKind value.</returns>
        public static CodeIssueKind ToCodeIssueKind(IssueKind issueKind)
        {
            switch (issueKind)
            {
                case (IssueKind.Error):
                    return CodeIssueKind.Error;
                case (IssueKind.Info):
                    return CodeIssueKind.Info;
                case (IssueKind.Unnecessary):
                    return CodeIssueKind.Unnecessary;
                case (IssueKind.Warning):
                    return CodeIssueKind.Warning;
                default:
                    throw new InvalidCastException(
                        string.Format("Cannot convert {0} to Roslyn.Services.CodeIssueKind.", issueKind.ToString()));
            }
        }
    }
}
