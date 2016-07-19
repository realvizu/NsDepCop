using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Implementation.Roslyn;
using Codartis.NsDepCop.Core.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

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
                    ReportConfigException(context, projectAnalyzer.ConfigFileName, projectAnalyzer.ConfigException);
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

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, DependencyViolation dependencyViolation, IssueKind issueKind)
        {
            var message = Constants.IllegalDependencyIssue.GetDynamicDescription(dependencyViolation);
            var severity = issueKind.ToDiagnosticSeverity();
            var warningLevel = severity == DiagnosticSeverity.Error ? 0 : 1;

            return Diagnostic.Create(
                IllegalDependencyDescriptor.Id,
                IllegalDependencyDescriptor.Category,
                message,
                severity: severity,
                defaultSeverity: IllegalDependencyDescriptor.DefaultSeverity,
                isEnabledByDefault: true,
                warningLevel: warningLevel,
                location: Location.Create(node.SyntaxTree, node.Span),
                helpLink: IllegalDependencyDescriptor.HelpLinkUri,
                title: IllegalDependencyDescriptor.Title);
        }

        private static void ReportConfigException(SyntaxNodeAnalysisContext context, string configFileName, Exception exception)
        {
            var diagnostic = CreateConfigExceptionDiagnostic(configFileName, exception);
            context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic CreateConfigExceptionDiagnostic(string configFileName, Exception exception)
        {
            var location = Location.Create(configFileName, new TextSpan(), new LinePositionSpan());
            return Diagnostic.Create(ConfigExceptionDescriptor, location, exception.Message);
        }
    }
}