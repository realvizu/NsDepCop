using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.Shell.Interop;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Base class for diagnostic analyzer implementation with different Roslyn versions.
    /// </summary>
    /// <remarks>
    /// The supporting classes (eg. DependencyAnalyzerProvider) are not thread safe! 
    /// However Roslyn assumes that all non-built-in diagnostic analyzers are not thread safe and invokes them accordingly.
    /// Should Roslyn behavior change this topic must be revisited.
    /// </remarks>
    public abstract class NsDepCopDiagnosticAnalyzerBase : DiagnosticAnalyzer, IDisposable
    {
        private static readonly TimeSpan AnalyzerCachingTimeSpan = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan ProjectFileCachingTimeSpan = TimeSpan.FromMilliseconds(2000);

        private readonly IProjectFileResolver _projectFileResolver;
        private readonly IAnalyzerProvider _analyzerProvider;

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

        protected NsDepCopDiagnosticAnalyzerBase(ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            if (typeDependencyEnumerator == null)
                throw new ArgumentNullException(nameof(typeDependencyEnumerator));

            _analyzerProvider = CreateDependencyAnalyzerProvider(typeDependencyEnumerator);
            _projectFileResolver = CreateProjectFileResolver();
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
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNodeAndLogException,
                SyntaxKind.IdentifierName,
                SyntaxKind.GenericName);

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }

        private void AnalyzeSyntaxNodeAndLogException(SyntaxNodeAnalysisContext context)
        {
            try
            {
                AnalyzeSyntaxNode(context);
            }
            catch (Exception e)
            {
                LogExceptionToActivityLog(e);
            }
        }

        private void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
        {
            if (context.Node?.SyntaxTree?.FilePath == null ||
                context.SemanticModel?.Compilation?.Assembly == null)
                return;

            var syntaxNode = context.Node;
            var semanticModel = context.SemanticModel;
            var assemblyName = semanticModel.Compilation.AssemblyName;

            var projectFilePath = _projectFileResolver.FindByAssemblyName(assemblyName);
            if (projectFilePath == null)
                return;

            var dependencyAnalyzer = _analyzerProvider.GetDependencyAnalyzer(projectFilePath);
            if (dependencyAnalyzer == null)
                return;

            var configState = dependencyAnalyzer.ConfigState;
            switch (configState)
            {
                case AnalyzerConfigState.NoConfig:
                    break;

                case AnalyzerConfigState.ConfigError:
                    ReportConfigException(context, dependencyAnalyzer.ConfigException);
                    break;

                case AnalyzerConfigState.Enabled:
                    var config = dependencyAnalyzer.Config;
                    var illegalDependencies = dependencyAnalyzer.AnalyzeSyntaxNode(new RoslynSyntaxNode(syntaxNode), new RoslynSemanticModel(semanticModel));
                    ReportIllegalDependencies(illegalDependencies, context, config.IssueKind, config.MaxIssueCount);
                    break;

                case AnalyzerConfigState.Disabled:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(configState), configState, "Unexpected value.");
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

        private static IProjectFileResolver CreateProjectFileResolver()
        {
            var projectFileResolver = new WorkspaceProjectFileResolver(LogTraceMessage);
            return new CachingProjectFileResolver(projectFileResolver, new DateTimeProvider(), ProjectFileCachingTimeSpan);
        }

        private static CachingAnalyzerProvider CreateDependencyAnalyzerProvider(ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            var dependencyAnalyzerFactory = new ConfiguredDependencyAnalyzerFactory(typeDependencyEnumerator, LogTraceMessage);
            var analyzerProvider = new AnalyzerProvider(dependencyAnalyzerFactory);
            return new CachingAnalyzerProvider(analyzerProvider, new DateTimeProvider(), AnalyzerCachingTimeSpan);
        }

        private static void LogExceptionToActivityLog(Exception exception)
        {
            var vsActivityLog = VisualStudioServiceGateway.GetActivityLogService();
            vsActivityLog.LogEntry((uint)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ProductConstants.ToolName, exception.ToString());
        }

        protected static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}