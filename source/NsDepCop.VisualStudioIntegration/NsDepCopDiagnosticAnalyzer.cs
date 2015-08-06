using System;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
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
        private static readonly DiagnosticDescriptor _illegalDependencyDescriptor =
            Constants.IllegalDependencyIssue.ToDiagnosticDescriptor();

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor _configExceptionDescriptor = 
            Constants.ConfigExceptionIssue.ToDiagnosticDescriptor();

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    _illegalDependencyDescriptor,
                    _configExceptionDescriptor);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode,
                SyntaxKind.IdentifierName,
                SyntaxKind.GenericName,
                SyntaxKind.ElementAccessExpression);
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node == null ||
                context.SemanticModel == null)
                return;

            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;

            if (syntaxNode.SyntaxTree == null ||
                string.IsNullOrWhiteSpace(syntaxNode.SyntaxTree.FilePath))
                return;

            var sourceFilePath = syntaxNode.SyntaxTree.FilePath;

            if (semanticModel.Compilation == null ||
                string.IsNullOrWhiteSpace(semanticModel.Compilation.AssemblyName))
                return;

            var assemblyName = semanticModel.Compilation.AssemblyName;

            var projectAnalyzer = ProjectAnalyzerRepository.GetAnalyzer(sourceFilePath, assemblyName);

            if (projectAnalyzer == null ||
                projectAnalyzer.State == ProjectAnalyzerState.Disabled)
                return;

            if (projectAnalyzer.State == ProjectAnalyzerState.ConfigError)
            {
                ReportConfigException(context, projectAnalyzer.ConfigException);
                return;
            }

            var dependencyViolations = projectAnalyzer.AnalyzeNode(syntaxNode, semanticModel);

            foreach (var dependencyViolation in dependencyViolations)
                ReportIllegalDependency(context, dependencyViolation, projectAnalyzer.IssueKind);
        }

        private void ReportIllegalDependency(SyntaxNodeAnalysisContext context, DependencyViolation dependencyViolation, IssueKind issueKind)
        {
            var diagnostic = CreateIllegalDependencyDiagnostic(context.Node, dependencyViolation, issueKind);
            context.ReportDiagnostic(diagnostic);
        }

        private Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, DependencyViolation dependencyViolation, IssueKind issueKind)
        {
            var message = string.Format(_illegalDependencyDescriptor.MessageFormat.ToString(),
                dependencyViolation.IllegalDependency.From,
                dependencyViolation.IllegalDependency.To,
                dependencyViolation.ReferencingTypeName,
                dependencyViolation.SourceSegment.Text,
                dependencyViolation.ReferencedTypeName);

            var severity = issueKind.ToDiagnosticSeverity();

            var warningLevel = severity == DiagnosticSeverity.Error ? 0 : 1;

            return Diagnostic.Create(
                _illegalDependencyDescriptor.Id,
                _illegalDependencyDescriptor.Category,
                message,
                severity: severity,
                defaultSeverity: _illegalDependencyDescriptor.DefaultSeverity,
                isEnabledByDefault: true,
                warningLevel: warningLevel,
                location: Location.Create(node.SyntaxTree, node.Span),
                helpLink: _illegalDependencyDescriptor.HelpLinkUri,
                title: _illegalDependencyDescriptor.Title);
        }

        private void ReportConfigException(SyntaxNodeAnalysisContext context, Exception exception)
        {
            var diagnostic = CreateConfigExceptionDiagnostic(context.Node, exception);
            context.ReportDiagnostic(diagnostic);
        }

        private Diagnostic CreateConfigExceptionDiagnostic(SyntaxNode node, Exception exception)
        {
            // The location should be the config.nsdepcop file, but we cannot use that because of a Roslyn 1.0 limitation: 
            // https://github.com/dotnet/roslyn/issues/3748#issuecomment-117231706
            // So we report the current syntax node's location.
            var location = Location.Create(node.SyntaxTree, node.Span);
            return Diagnostic.Create(_configExceptionDescriptor, location, exception.Message);
        }
    }
}