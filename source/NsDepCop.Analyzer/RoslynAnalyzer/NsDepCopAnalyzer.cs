﻿using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Analysis.Factory;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Config.Factory;
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
    // ReSharper disable once UnusedType.Global
    public sealed class NsDepCopAnalyzer : DiagnosticAnalyzer
    {
        private static readonly ImmutableArray<DiagnosticDescriptor> DiagnosticDescriptors =
            ImmutableArray.Create(
                DiagnosticDefinitions.IllegalDependency,
                DiagnosticDefinitions.TooManyDependencyIssues,
                DiagnosticDefinitions.NoConfigFile,
                DiagnosticDefinitions.ConfigDisabled,
                DiagnosticDefinitions.ConfigException,
                DiagnosticDefinitions.ToolDisabled,
                DiagnosticDefinitions.IllegalAssemblyDependency
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
                new AssemblyDependencyAnalyzerFactory(),
                new ConfigProviderFactory(LogTraceMessage),
                new TypeDependencyEnumerator(new SyntaxNodeAnalyzer(), LogTraceMessage)
            );
        }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticDescriptors;

        public override void Initialize(AnalysisContext analysisContext)
        {
            analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            analysisContext.EnableConcurrentExecution();

            analysisContext.RegisterCompilationStartAction(OnCompilationStart);
            analysisContext.RegisterCompilationAction(OnCompilation);
        }

        private void OnCompilationStart(CompilationStartAnalysisContext compilationStartContext)
        {
            if (GlobalSettings.IsToolDisabled())
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, DiagnosticDefinitions.ToolDisabled));
                return;
            }

            var configFilePath = GetConfigFilePath(compilationStartContext.Options.AdditionalFiles);
            if (configFilePath == null || !File.Exists(configFilePath))
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, DiagnosticDefinitions.NoConfigFile));
                return;
            }

            var dependencyAnalyzer = _analyzerProvider.GetDependencyAnalyzer(configFilePath);
            if (dependencyAnalyzer.ConfigState == AnalyzerConfigState.ConfigError)
            {
                var exceptionMessage = dependencyAnalyzer.ConfigException.Message;
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, DiagnosticDefinitions.ConfigException, exceptionMessage));
                return;
            }

            if (dependencyAnalyzer.ConfigState == AnalyzerConfigState.Disabled)
            {
                compilationStartContext.RegisterSyntaxTreeAction(i => ReportForSyntaxTree(i, DiagnosticDefinitions.ConfigDisabled));
                return;
            }

            // Per-compilation dependency issue counter.
            var issueCount = 0;

            compilationStartContext.RegisterSyntaxNodeAction(
                i => AnalyzeSyntaxNodeAndReportDiagnostics(i, dependencyAnalyzer, ref issueCount),
                SyntaxKindsToRegister);
        }

        private static void AnalyzeSyntaxNodeAndReportDiagnostics(
            SyntaxNodeAnalysisContext syntaxNodeAnalysisContext,
            IDependencyAnalyzer dependencyAnalyzer,
            ref int issueCount)
        {
            var maxIssueCount = dependencyAnalyzer.Config.MaxIssueCount;

            // Not sure whether this method will be called concurrently so to be on the safe side let's access issueCount with interlocked.
            if (GetInterlocked(ref issueCount) > maxIssueCount)
                return;

            var analyzerMessages = dependencyAnalyzer.AnalyzeSyntaxNode(syntaxNodeAnalysisContext.Node, syntaxNodeAnalysisContext.SemanticModel);

            foreach (var analyzerMessage in analyzerMessages.OfType<IllegalDependencyMessage>())
            {
                var currentIssueCount = Interlocked.Increment(ref issueCount);

                if (currentIssueCount > maxIssueCount)
                {
                    ReportForSyntaxNode(syntaxNodeAnalysisContext, DiagnosticDefinitions.TooManyDependencyIssues, maxIssueCount);
                    break;
                }

                string allowedMemberNames = string.Empty;

                if (analyzerMessage.AllowedMemberNames.Length > 0)
                {
                    allowedMemberNames = $" Allowed types are: {string.Join(", ", analyzerMessage.AllowedMemberNames)}";
                }

                ReportForSyntaxNode(
                    syntaxNodeAnalysisContext,
                    DiagnosticDefinitions.IllegalDependency,
                    analyzerMessage.IllegalDependency.FromNamespaceName,
                    analyzerMessage.IllegalDependency.ToNamespaceName,
                    analyzerMessage.IllegalDependency.FromTypeName,
                    analyzerMessage.IllegalDependency.ToTypeName,
                    allowedMemberNames
                );
            }
        }

        /// <remarks>
        /// Reading an int that's updated by Interlocked on other threads: https://stackoverflow.com/a/24893231/38186
        /// </remarks>
        private static int GetInterlocked(ref int issueCount) => Interlocked.CompareExchange(ref issueCount, 0, 0);

        private void OnCompilation(CompilationAnalysisContext compilationAnalysisContext)
        {
            if (GlobalSettings.IsToolDisabled())
            {
                ReportForCompilationAnalysisContext(compilationAnalysisContext, DiagnosticDefinitions.ToolDisabled);
                return;
            }

            var configFilePath = GetConfigFilePath(compilationAnalysisContext.Options.AdditionalFiles);
            if (configFilePath == null || !File.Exists(configFilePath))
            {
                ReportForCompilationAnalysisContext(compilationAnalysisContext, DiagnosticDefinitions.NoConfigFile);
                return;
            }

            var assemblyDependencyAnalyzer = _analyzerProvider.GetAssemblyDependencyAnalyzer(configFilePath);
            if (assemblyDependencyAnalyzer.ConfigState == AnalyzerConfigState.ConfigError)
            {
                var exceptionMessage = assemblyDependencyAnalyzer.ConfigException.Message;
                ReportForCompilationAnalysisContext(compilationAnalysisContext, DiagnosticDefinitions.ConfigException, exceptionMessage);
                return;
            }

            if (assemblyDependencyAnalyzer.ConfigState == AnalyzerConfigState.Disabled)
            {
                ReportForCompilationAnalysisContext(compilationAnalysisContext, DiagnosticDefinitions.ConfigDisabled);
                return;
            }

            AssemblyIdentity sourceAssembly = compilationAnalysisContext.Compilation.Assembly.Identity;
            ImmutableArray<AssemblyIdentity> referencedAssemblies
                = compilationAnalysisContext.Compilation.ReferencedAssemblyNames.ToImmutableArray();
            var analyzerMessages = assemblyDependencyAnalyzer.AnalyzeProject(sourceAssembly, referencedAssemblies);

            foreach (var illegalAssemblyDependencyMessage in analyzerMessages.OfType<IllegalAssemblyDependencyMessage>())
            {
                ReportForCompilationAnalysisContext(
                    compilationAnalysisContext,
                    DiagnosticDefinitions.IllegalAssemblyDependency,
                    illegalAssemblyDependencyMessage.IllegalAssemblyDependency.FromAssembly.Name,
                    illegalAssemblyDependencyMessage.IllegalAssemblyDependency.ToAssembly.Name
                );
            }
        }

        private static void ReportForSyntaxTree(
            SyntaxTreeAnalysisContext syntaxTreeAnalysisContext,
            DiagnosticDescriptor diagnosticDescriptor,
            params object[] messageArgs)
        {
            var location = Location.Create(syntaxTreeAnalysisContext.Tree, TextSpan.FromBounds(0, 0));
            var diagnostic = CreateDiagnostic(diagnosticDescriptor, location, messageArgs);
            syntaxTreeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static void ReportForSyntaxNode(
            SyntaxNodeAnalysisContext syntaxNodeAnalysisContext,
            DiagnosticDescriptor diagnosticDescriptor,
            params object[] messageArgs)
        {
            var node = syntaxNodeAnalysisContext.Node;
            var location = Location.Create(node.SyntaxTree, node.Span);
            var diagnostic = CreateDiagnostic(diagnosticDescriptor, location, messageArgs);
            syntaxNodeAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static void ReportForCompilationAnalysisContext(
            CompilationAnalysisContext compilationAnalysisContext,
            DiagnosticDescriptor diagnosticDescriptor,
            params object[] messageArguments)
        {
            var diagnostic = CreateDiagnostic(diagnosticDescriptor, null, messageArguments);
            compilationAnalysisContext.ReportDiagnostic(diagnostic);
        }

        private static Diagnostic CreateDiagnostic(DiagnosticDescriptor diagnosticDescriptor, Location location, params object[] messageArgs)
        {
            return Diagnostic.Create(diagnosticDescriptor, location, messageArgs);
        }

        private static string GetConfigFilePath(ImmutableArray<AdditionalText> additionalFiles)
        {
            return additionalFiles.FirstOrDefault(IsConfigFile)?.Path;
        }

        private static bool IsConfigFile(AdditionalText additionalText)
        {
            return string.Equals(Path.GetFileName(additionalText.Path), ProductConstants.DefaultConfigFileName, StringComparison.OrdinalIgnoreCase);
        }

        private static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}