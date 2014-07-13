using Codartis.NsDepCop.Core.Common;
using Roslyn.Services;

namespace Codartis.NsDepCop.Core.Analyzer.Roslyn
{
    /// <summary>
    /// Implements extension methods for Codartis.NsDepCop.Core.IssueKind.
    /// </summary>
    public static class IssueKindExtension
    {
        /// <summary>
        /// Translates from Codartis.NsDepCop.Core.IssueKind to Roslyn.Services.CodeIssueKind.
        /// </summary>
        /// <param name="issueKind">A Codartis.NsDepCop.Core.IssueKind value.</param>
        /// <returns>A Roslyn.Services.CodeIssueKind value.</returns>
        public static CodeIssueKind ToCodeIssueKind(this IssueKind issueKind)
        {
            return IssueKindTranslator.ToCodeIssueKind(issueKind);
        }
    }
}
