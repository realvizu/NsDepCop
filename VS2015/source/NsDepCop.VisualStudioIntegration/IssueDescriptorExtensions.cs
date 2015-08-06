using System.Globalization;
using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    public static class IssueDescriptorExtensions
    {
        /// <summary>
        /// Format string to create a help link for diagnostics. The parameter is the diagnostic's code in full UPPERCASE.
        /// </summary>
        private const string HELP_LINK_FORMAT = @"https://nsdepcop.codeplex.com/wikipage?title=Diagnostics#{0}";

        public static DiagnosticDescriptor ToDiagnosticDescriptor(this IssueDescriptor issueDescriptor)
        {
            return new DiagnosticDescriptor(
                issueDescriptor.Id,
                issueDescriptor.Description,
                issueDescriptor.MessageFormat,
                Constants.TOOL_NAME,
                issueDescriptor.DefaultKind.ToDiagnosticSeverity(),
                true,
                helpLinkUri: GetHelpLink(issueDescriptor.Id));
        }

        private static string GetHelpLink(string issueDescriptorId)
        {
            return string.Format(HELP_LINK_FORMAT, issueDescriptorId.ToUpper(CultureInfo.InvariantCulture));
        }
    }
}
