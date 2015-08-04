using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    public static class IssueDescriptorExtensions
    {
        public static DiagnosticDescriptor ToDiagnosticDescriptor(this IssueDescriptor issueDescriptor)
        {
            return new DiagnosticDescriptor(
                issueDescriptor.Id,
                issueDescriptor.Description,
                issueDescriptor.MessageFormat,
                Constants.TOOL_NAME,
                issueDescriptor.DefaultKind.ToDiagnosticSeverity(),
                true);
        }
    }
}
