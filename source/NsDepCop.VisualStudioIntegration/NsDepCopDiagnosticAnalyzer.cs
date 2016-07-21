using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Implementation.Roslyn;
using Codartis.NsDepCop.Core.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// This diagnostic analyzer is invoked by Visual Studio/Roslyn an it reports namespace dependency issues to the VS IDE.
    /// </summary>
    /// <remarks>
    /// The supporting classes (eg. ProjectAnalyzerConfigRepository) are not thread safe! 
    /// However Roslyn 1.0 assumes that all non-built-in diagnostic analyzers are not thread safe and invokes them accordingly.
    /// Should Roslyn behavior change this topic must be revisited.
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NsDepCopDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Descriptor for the 'Illegal namespace dependency' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor IllegalDependencyDescriptor =
            Constants.IllegalDependencyIssue.ToDiagnosticDescriptor();

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor ConfigExceptionDescriptor =
            Constants.ConfigExceptionIssue.ToDiagnosticDescriptor();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                IllegalDependencyDescriptor,
                ConfigExceptionDescriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode,
                SyntaxKind.IdentifierName,
                SyntaxKind.GenericName,
                SyntaxKind.ElementAccessExpression);
        }

        private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node == null ||
                context.SemanticModel == null)
                return;

            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;

            if (string.IsNullOrWhiteSpace(syntaxNode.SyntaxTree?.FilePath))
                return;

            var sourceFilePath = syntaxNode.SyntaxTree.FilePath;

            if (string.IsNullOrWhiteSpace(semanticModel.Compilation?.AssemblyName))
                return;

            var assemblyName = semanticModel.Compilation.AssemblyName;

            var projectAnalyzer = ProjectDependencyAnalyzerRepository.GetAnalyzer(sourceFilePath, assemblyName);
            if (projectAnalyzer == null)
                return;

            switch (projectAnalyzer.State)
            {
                case DependencyAnalyzerState.NoConfigFile:
                case DependencyAnalyzerState.Disabled:
                    break;

                case DependencyAnalyzerState.ConfigError:
                    ReportConfigException(context, projectAnalyzer.ConfigException);
                    break;

                case DependencyAnalyzerState.Enabled:
                    var dependencyViolations = projectAnalyzer.AnalyzeNode(syntaxNode, semanticModel);
                    ReportIllegalDependencies(dependencyViolations, context, projectAnalyzer.DependencyViolationIssueKind);
                    break;

                default:
                    throw new Exception($"Unexpected DependencyAnalyzerState {projectAnalyzer.State}");
            }
        }

        private static void ReportIllegalDependencies(IEnumerable<DependencyViolation> dependencyViolations,
            SyntaxNodeAnalysisContext context, IssueKind issueKind)
        {
            foreach (var dependencyViolation in dependencyViolations)
            {
                var diagnostic = CreateIllegalDependencyDiagnostic(context.Node, dependencyViolation, issueKind);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static void ReportConfigException(SyntaxNodeAnalysisContext context, Exception exception)
        {
            var diagnostic = CreateConfigExceptionDiagnostic(context.Node, exception);
            context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, DependencyViolation dependencyViolation, IssueKind issueKind)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            var message = Constants.IllegalDependencyIssue.GetDynamicDescription(dependencyViolation);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message, issueKind);
        }

        private static Diagnostic CreateConfigExceptionDiagnostic(SyntaxNode node, Exception exception)
        {
            // The location should be the config.nsdepcop file, but we cannot use that because of a Roslyn limitation: 
            // https://github.com/dotnet/roslyn/issues/6649
            // So we report the current syntax node's location.
            var location = Location.Create(node.SyntaxTree, node.Span);
            var message = Constants.ConfigExceptionIssue.GetDynamicDescription(exception);
            return CreateDiagnostic(ConfigExceptionDescriptor, location, message);
        }

        private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, 
            Location location, string message, IssueKind? issueKind = null)
        {
            var severity = issueKind?.ToDiagnosticSeverity() ?? diagnosticDescriptor.DefaultSeverity;

            return Diagnostic.Create(
                diagnosticDescriptor.Id,
                diagnosticDescriptor.Category,
                message,
                severity: severity,
                defaultSeverity: diagnosticDescriptor.DefaultSeverity,
                isEnabledByDefault: true,
                warningLevel: GetWarningLevel(severity),
                location: location,
                helpLink: diagnosticDescriptor.HelpLinkUri,
                title: diagnosticDescriptor.Title);
        }

        private static int GetWarningLevel(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : 1;
    }
}