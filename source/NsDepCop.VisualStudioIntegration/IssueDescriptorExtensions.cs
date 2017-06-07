using System.Globalization;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    internal static class IssueDescriptorExtensions
    {
        /// <summary>
        /// Format string to create a help link for diagnostics. The parameter is the diagnostic's code in lowercase.
        /// </summary>
        private const string HelpLinkFormat = @"https://github.com/realvizu/NsDepCop/blob/VS2015/doc/Diagnostics.md#{0}";

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
            return string.Format(HelpLinkFormat, issueDescriptorId.ToLower(CultureInfo.InvariantCulture));
        }
    }
}
