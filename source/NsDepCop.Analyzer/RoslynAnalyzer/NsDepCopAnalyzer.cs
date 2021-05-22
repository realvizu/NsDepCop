using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Factory;
using Codartis.NsDepCop.Implementation;
using Codartis.NsDepCop.Interface;
using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Interface.Analysis.Messages;
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
        private static readonly DiagnosticDescriptor ConfigDisabledDescriptor = IssueDefinitions.ConfigDisabledIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor ConfigExceptionDescriptor = IssueDefinitions.ConfigExceptionIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor ToolDisabledDescriptor = IssueDefinitions.ToolDisabledIssue.ToDiagnosticDescriptor();

        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors =
            ImmutableArray.Create(
                IllegalDependencyDescriptor,
                TooManyIssuesDescriptor,
                NoConfigFileDescriptor,
                ConfigDisabledDescriptor,
                ConfigExceptionDescriptor,
                ToolDisabledDescriptor
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
            if (GlobalSettings.IsToolDisabled())
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, ToolDisabledDescriptor));
                return;
            }

            var configFilePath = GetConfigFilePath(compilationStartContext.Options.AdditionalFiles);
            if (configFilePath == null)
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, NoConfigFileDescriptor));
                return;
            }

            var dependencyAnalyzer = _analyzerProvider.GetDependencyAnalyzer(configFilePath);
            if (dependencyAnalyzer == null)
                throw new Exception($"Could not acquire DependencyAnalyzer for path: '{configFilePath}'");

            if (dependencyAnalyzer.HasConfigError)
            {
                var message = dependencyAnalyzer.GetConfigException().ToString();
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, ConfigExceptionDescriptor, message));
                return;
            }

            if (dependencyAnalyzer.IsDisabledInConfig)
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, ConfigDisabledDescriptor));
                return;
            }

            // Per-compilation dependency issue counter.
            var issueCount = 0;

            compilationStartContext.RegisterSyntaxNodeAction(
                i => AnalyzeSyntaxNodeAndReportDiagnostics(i, dependencyAnalyzer, ref issueCount),
                SyntaxKindsToRegister);
        }


        private static void ReportForSyntaxTree(
            SyntaxTreeAnalysisContext syntaxTreeAnalysisContext,
            DiagnosticDescriptor diagnosticDescriptor,
            string message = null)
        {
            var diagnostic = CreateDiagnosticForSyntaxTree(syntaxTreeAnalysisContext.Tree, diagnosticDescriptor, message);
            syntaxTreeAnalysisContext.ReportDiagnostic(diagnostic);
        }


        private static void AnalyzeSyntaxNodeAndReportDiagnostics(
            SyntaxNodeAnalysisContext syntaxNodeAnalysisContext,
            IDependencyAnalyzer dependencyAnalyzer,
            ref int issueCount)
        {
            var analyzerMessages = dependencyAnalyzer.AnalyzeSyntaxNode(
                syntaxNodeAnalysisContext.Node,
                syntaxNodeAnalysisContext.SemanticModel,
                ref issueCount
            );

            foreach (var analyzerMessage in analyzerMessages)
            {
                var diagnostic = ConvertAnalyzerMessageToDiagnostic(syntaxNodeAnalysisContext, analyzerMessage);
                syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
            }
        }

        private static Diagnostic ConvertAnalyzerMessageToDiagnostic(SyntaxNodeAnalysisContext syntaxNodeAnalysisContext, AnalyzerMessageBase analyzerMessage)
        {
            return analyzerMessage switch
            {
                IllegalDependencyMessage _ =>
                    CreateDiagnosticForSyntaxNode(syntaxNodeAnalysisContext.Node, IllegalDependencyDescriptor, analyzerMessage.ToString()),

                TooManyIssuesMessage _ =>
                    CreateDiagnosticForSyntaxNode(syntaxNodeAnalysisContext.Node, TooManyIssuesDescriptor, analyzerMessage.ToString()),

                // All other possibilities were ruled out in StartAnalysisForCompilation (no config, tool disabled, etc.)

                _ => throw new Exception($"Unexpected analyzer message type: {analyzerMessage?.GetType().Name}")
            };
        }


        private static Diagnostic CreateDiagnosticForSyntaxTree(SyntaxTree syntaxTree, DiagnosticDescriptor diagnosticDescriptor, string message)
        {
            var location = Location.Create(syntaxTree, TextSpan.FromBounds(0, 0));
            return CreateDiagnostic(diagnosticDescriptor, location, message);
        }

        private static Diagnostic CreateDiagnosticForSyntaxNode(SyntaxNode node, DiagnosticDescriptor diagnosticDescriptor, string message)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(diagnosticDescriptor, location, message);
        }

        private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location, string message)
        {
            var severity = diagnosticDescriptor.DefaultSeverity;

            return Diagnostic.Create(
                diagnosticDescriptor.Id,
                diagnosticDescriptor.Category,
                message: message ?? diagnosticDescriptor.Title,
                severity: severity,
                defaultSeverity: diagnosticDescriptor.DefaultSeverity,
                isEnabledByDefault: true,
                warningLevel: GetWarningLevel(severity),
                location: location,
                helpLink: diagnosticDescriptor.HelpLinkUri,
                title: diagnosticDescriptor.Title
                // Cannot use description param, because command line output does not show. Otherwise it would be nice because VS shows it behind and accordion.
            );
        }

        private static string GetConfigFilePath(ImmutableArray<AdditionalText> additionalFiles)
        {
            return additionalFiles.FirstOrDefault(IsConfigFile)?.Path;
        }

        private static bool IsConfigFile(AdditionalText additionalText)
        {
            return string.Equals(Path.GetFileName(additionalText.Path), ProductConstants.DefaultConfigFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetWarningLevel(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : 1;

        private static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}