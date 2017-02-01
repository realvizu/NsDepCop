using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Defines issue descriptors.
    /// </summary>
    public static class IssueDefinitions
    {
        public static readonly IssueDescriptor<DependencyViolation> IllegalDependencyIssue =
            new IssueDescriptor<DependencyViolation>(
                "NSDEPCOP01",
                IssueKind.Warning,
                "Illegal namespace reference.",
                i => $"Illegal namespace reference: {i?.TypeDependency.FromNamespaceName}->{i?.TypeDependency.ToNamespaceName} (Type: {i?.TypeDependency.FromTypeName}->{i?.TypeDependency.ToTypeName})");

        public static readonly IssueDescriptor TooManyIssuesIssue = 
            new IssueDescriptor(
                "NSDEPCOP02",
                IssueKind.Warning,
                "Too many issues, analysis was stopped.");

        public static readonly IssueDescriptor NoConfigFileIssue = 
            new IssueDescriptor(
                "NSDEPCOP03",
                IssueKind.Info,
                "No config file found, analysis skipped.");

        public static readonly IssueDescriptor ConfigDisabledIssue = 
            new IssueDescriptor(
                "NSDEPCOP04",
                IssueKind.Info,
                "Analysis is disabled in the nsdepcop config file.");

        public static readonly IssueDescriptor<Exception> ConfigExceptionIssue = 
            new IssueDescriptor<Exception>(
                "NSDEPCOP05",
                IssueKind.Error,
                "Error loading NsDepCop config.",
                i => $"{i?.Message}");
    }
}