using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Roslyn;
using Codartis.NsDepCop.Core.Interface.Config;
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
    public class NsDepCopDiagnosticAnalyzer : DiagnosticAnalyzer, IDisposable
    {
        private readonly ProjectAnalyzerProvider _analyzerProvider;

        /// <summary>
        /// Descriptor for the 'Illegal namespace dependency' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor IllegalDependencyDescriptor =
            IssueDefinitions.IllegalDependencyIssue.ToDiagnosticDescriptor();

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor ConfigExceptionDescriptor =
            IssueDefinitions.ConfigExceptionIssue.ToDiagnosticDescriptor();

        public NsDepCopDiagnosticAnalyzer()
        {
            _analyzerProvider = new ProjectAnalyzerProvider();
        }

        public void Dispose()
        {
            _analyzerProvider.Dispose();
        }

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

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node?.SyntaxTree?.FilePath == null || 
                context.SemanticModel?.Compilation?.Assembly == null)
                return;

            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;
            var sourceFilePath = syntaxNode.SyntaxTree.FilePath;
            var assemblyName = semanticModel.Compilation.AssemblyName;

            var dependencyAnalyzer = _analyzerProvider.GetDependencyAnalyzer(sourceFilePath, assemblyName);
            if (dependencyAnalyzer == null)
                return;

            switch (dependencyAnalyzer.State)
            {
                case AnalyzerState.NoConfigFile:
                case AnalyzerState.Disabled:
                    break;

                case AnalyzerState.ConfigError:
                    ReportConfigException(context, dependencyAnalyzer.ConfigException);
                    break;

                case AnalyzerState.Enabled:
                    var config = dependencyAnalyzer.Config;
                    var illegalDependencies = dependencyAnalyzer.AnalyzeSyntaxNode(new RoslynSyntaxNode(syntaxNode), new RoslynSemanticModel(semanticModel));
                    ReportIllegalDependencies(illegalDependencies, context, config.IssueKind, config.MaxIssueCount);
                    break;

                default:
                    throw new Exception($"Unexpected ConfigState {dependencyAnalyzer.State}");
            }
        }

        private static void ReportIllegalDependencies(IEnumerable<TypeDependency> illegalDependencies,
            SyntaxNodeAnalysisContext context, IssueKind issueKind, int maxIssueCount)
        {
            var issueCount = 0;
            foreach (var typeDependency in illegalDependencies)
            {
                var diagnostic = CreateIllegalDependencyDiagnostic(context.Node, typeDependency, issueKind);
                context.ReportDiagnostic(diagnostic);
                issueCount++;
            }

            if (issueCount == maxIssueCount)
                context.ReportDiagnostic(CreateTooManyIssueDiagnostic(context.Node));
        }

        private static void ReportConfigException(SyntaxNodeAnalysisContext context, Exception exception)
        {
            var diagnostic = CreateConfigExceptionDiagnostic(context.Node, exception);
            context.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, TypeDependency typeDependency, IssueKind issueKind)
        {
            // TODO: get location from typeDependency.SourceSegment?
            var location = Location.Create(node.SyntaxTree, node.Span);
            var message = IssueDefinitions.IllegalDependencyIssue.GetDynamicDescription(typeDependency);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message, issueKind);
        }

        private static Diagnostic CreateTooManyIssueDiagnostic(SyntaxNode node)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            var message = IssueDefinitions.TooManyIssuesIssue.StaticDescription;
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message);
        }
        private static Diagnostic CreateConfigExceptionDiagnostic(SyntaxNode node, Exception exception)
        {
            // The location should be the config.nsdepcop file, but we cannot use that because of a Roslyn limitation: 
            // https://github.com/dotnet/roslyn/issues/6649
            // So we report the current syntax node's location.
            var location = Location.Create(node.SyntaxTree, node.Span);
            var message = IssueDefinitions.ConfigExceptionIssue.GetDynamicDescription(exception);
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