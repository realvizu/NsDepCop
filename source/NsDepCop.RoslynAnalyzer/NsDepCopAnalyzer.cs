using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.ParserAdapter.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    /// <summary>
    /// Wraps the dependency analyzer in a Roslyn <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class NsDepCopAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor IllegalDependencyDescriptor = IssueDefinitions.IllegalDependencyIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor TooManyIssuesDescriptor = IssueDefinitions.TooManyIssuesIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor NoConfigFileDescriptor = IssueDefinitions.NoConfigFileIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor ConfigExceptionDescriptor = IssueDefinitions.ConfigExceptionIssue.ToDiagnosticDescriptor();

        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors =
            ImmutableArray.Create(
                IllegalDependencyDescriptor,
                NoConfigFileDescriptor,
                ConfigExceptionDescriptor
            );

        private static readonly ImmutableArray<SyntaxKind> SyntaxKindsToRegister =
            ImmutableArray.Create(
                SyntaxKind.IdentifierName,
                SyntaxKind.GenericName,
                SyntaxKind.DefaultLiteralExpression
            );

        private readonly IAnalyzerProvider _analyzerProvider;

        public NsDepCopAnalyzer()
        {
            _analyzerProvider = new AnalyzerProvider(
                new DependencyAnalyzerFactory(LogTraceMessage),
                new TypeDependencyEnumerator(new SyntaxNodeAnalyzer(), LogTraceMessage)
            );
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticDescriptors;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.EnableConcurrentExecution();
            analysisContext.RegisterCompilationStartAction(StartAnalysisForCompilation);
        }

        private void StartAnalysisForCompilation(CompilationStartAnalysisContext compilationStartContext)
        {
            var configFilePath = compilationStartContext.Options.AdditionalFiles.FirstOrDefault(IsConfigFile)?.Path;
            if (configFilePath == null)
            {
                compilationStartContext.RegisterSyntaxTreeAction(ReportNoConfig);
                return;
            }

            var dependencyAnalyzer = _analyzerProvider.GetDependencyAnalyzer(configFilePath);
            if (dependencyAnalyzer == null)
                throw new Exception($"Could not acquire DependencyAnalyzer for path: '{configFilePath}'");

            compilationStartContext.RegisterSyntaxNodeAction(
                syntaxNodeAnalysisContext => { AnalyzeSyntaxNodeAndReportDiagnostics(dependencyAnalyzer, syntaxNodeAnalysisContext); },
                SyntaxKindsToRegister);
        }


        private static void AnalyzeSyntaxNodeAndReportDiagnostics(IDependencyAnalyzer dependencyAnalyzer, SyntaxNodeAnalysisContext syntaxNodeAnalysisContext)
        {
            var analyzerMessages = dependencyAnalyzer.AnalyzeSyntaxNode(
                new RoslynSyntaxNode(syntaxNodeAnalysisContext.Node),
                new RoslynSemanticModel(syntaxNodeAnalysisContext.SemanticModel)
            );

            foreach (var analyzerMessage in analyzerMessages)
            {
                var diagnostic = ConvertAnalyzerMessageToDiagnostic(syntaxNodeAnalysisContext.Node, analyzerMessage);
                if (diagnostic != null)
                    syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
            }
        }

        private static void ReportNoConfig(SyntaxTreeAnalysisContext syntaxTreeAnalysisContext)
        {
            var diagnostic = Diagnostic.Create(NoConfigFileDescriptor, Location.Create(syntaxTreeAnalysisContext.Tree, TextSpan.FromBounds(0, 0)));
            syntaxTreeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic ConvertAnalyzerMessageToDiagnostic(SyntaxNode node, AnalyzerMessageBase analyzerMessage)
        {
            return analyzerMessage switch
            {
                IllegalDependencyMessage illegalDependencyMessage =>
                    CreateIllegalDependencyDiagnostic(node, illegalDependencyMessage.ToString(), illegalDependencyMessage.IssueKind),

                TooManyIssuesMessage tooManyIssuesMessage =>
                    CreateTooManyIssuesDiagnostic(node, tooManyIssuesMessage.ToString(), tooManyIssuesMessage.IssueKind),

                ConfigErrorMessage configErrorMessage =>
                    CreateConfigExceptionDiagnostic(node, configErrorMessage.ToString()),

                _ => null
            };
        }

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, string message, IssueKind issueKind)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message, issueKind);
        }

        private static Diagnostic CreateTooManyIssuesDiagnostic(SyntaxNode node, string message, IssueKind issueKind)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(TooManyIssuesDescriptor, location, message, issueKind);
        }

        private static Diagnostic CreateConfigExceptionDiagnostic(SyntaxNode node, string message)
        {
            // The location should be the config.nsdepcop file, but we cannot use that because of a Roslyn limitation: 
            // https://github.com/dotnet/roslyn/issues/6649
            // So we report the current syntax node's location.
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(ConfigExceptionDescriptor, location, message);
        }

        private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location, string message, IssueKind? issueKind = null)
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

        private static bool IsConfigFile(AdditionalText additionalText)
        {
            return string.Equals(Path.GetFileName(additionalText.Path), ProductConstants.DefaultConfigFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetWarningLevel(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : 1;

        private static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}