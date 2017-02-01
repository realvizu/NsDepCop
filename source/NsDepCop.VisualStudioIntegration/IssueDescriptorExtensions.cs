using System.Globalization;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Roslyn;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    public static class IssueDescriptorExtensions
    {
        /// <summary>
        /// Format string to create a help link for diagnostics. The parameter is the diagnostic's code in full UPPERCASE.
        /// </summary>
        private const string HelpLinkFormat = @"https://nsdepcop.codeplex.com/wikipage?title=Diagnostics#{0}";

        public static DiagnosticDescriptor ToDiagnosticDescriptor<TIssue>(this IssueDescriptor<TIssue> issueDescriptor)
        {
            return new DiagnosticDescriptor(
                issueDescriptor.Id,
                issueDescriptor.StaticDescription,
                issueDescriptor.StaticDescription,
                ProductConstants.ToolName,
                issueDescriptor.DefaultKind.ToDiagnosticSeverity(),
                true,
                helpLinkUri: GetHelpLink(issueDescriptor.Id));
        }

        private static string GetHelpLink(string issueDescriptorId)
        {
            return string.Format(HelpLinkFormat, issueDescriptorId.ToUpper(CultureInfo.InvariantCulture));
        }
    }
}
