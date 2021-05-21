using System.Globalization;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    internal static class IssueDescriptorExtensions
    {
        /// <summary>
        /// Format string to create a help link for diagnostics. The parameter is the diagnostic's code in lowercase.
        /// </summary>
        private const string HelpLinkFormat = @"https://github.com/realvizu/NsDepCop/blob/master/doc/Diagnostics.md#{0}";

        public static DiagnosticDescriptor ToDiagnosticDescriptor(this IssueDescriptor issueDescriptor)
        {
            return new DiagnosticDescriptor(
                issueDescriptor.Id,
                issueDescriptor.Title,
                issueDescriptor.Title,
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
