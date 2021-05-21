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
        private static readonly DiagnosticDescriptor NoConfigFileDescriptor = IssueDefinitions.NoConfigFileIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor ConfigExceptionDescriptor = IssueDefinitions.ConfigExceptionIssue.ToDiagnosticDescriptor();
        private static readonly DiagnosticDescriptor ToolDisabledDescriptor = IssueDefinitions.ToolDisabledIssue.ToDiagnosticDescriptor();

        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors =
            ImmutableArray.Create(
                IllegalDependencyDescriptor,
                NoConfigFileDescriptor,
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

            var configFilePath = compilationStartContext.Options.AdditionalFiles.FirstOrDefault(IsConfigFile)?.Path;
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

            compilationStartContext.RegisterSyntaxNodeAction(
                i => AnalyzeSyntaxNodeAndReportDiagnostics(i, dependencyAnalyzer),
                SyntaxKindsToRegister);
        }

        private static void ReportForSyntaxTree(
            SyntaxTreeAnalysisContext syntaxTreeAnalysisContext,
            DiagnosticDescriptor diagnosticDescriptor,
            string message = null)
        {
            var location = Location.Create(syntaxTreeAnalysisContext.Tree, TextSpan.FromBounds(0, 0));
            var diagnostic = CreateDiagnostic(diagnosticDescriptor, location, message);
            syntaxTreeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static void AnalyzeSyntaxNodeAndReportDiagnostics(
            SyntaxNodeAnalysisContext syntaxNodeAnalysisContext,
            IDependencyAnalyzer dependencyAnalyzer)
        {
            var analyzerMessages = dependencyAnalyzer.AnalyzeSyntaxNode(
                new RoslynSyntaxNode(syntaxNodeAnalysisContext.Node),
                new RoslynSemanticModel(syntaxNodeAnalysisContext.SemanticModel)
            );

            var diagnostics = analyzerMessages.OfType<IllegalDependencyMessage>()
                .Select(i => CreateIllegalDependencyDiagnostic(syntaxNodeAnalysisContext.Node, i.ToString()));

            foreach (var diagnostic in diagnostics)
                syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, string message)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message);
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

        private static bool IsConfigFile(AdditionalText additionalText)
        {
            return string.Equals(Path.GetFileName(additionalText.Path), ProductConstants.DefaultConfigFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetWarningLevel(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : 1;

        private static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}