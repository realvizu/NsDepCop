// Diagnostic RS 1022 (Do not use types from Workspaces assembly in an analyzer) is disabled
// because I don't have any other idea how to find the config.nsdepcop file for a syntax node.
#pragma warning disable RS1022

using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.Shell;
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
            context.RegisterSyntaxNodeAction(AnalyzeSyntaxNodeAndLogException, GetSyntaxKindsToRegister());
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        }

        protected abstract SyntaxKind[] GetSyntaxKindsToRegister();

        private void AnalyzeSyntaxNodeAndLogException(SyntaxNodeAnalysisContext context)
        {
            try
            {
                AnalyzeSyntaxNode(context);
            }
            catch (Exception e)
            {
                ThreadHelper.ThrowIfNotOnUIThread();
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

            var analyzerMessages = dependencyAnalyzer.AnalyzeSyntaxNode(new RoslynSyntaxNode(syntaxNode), new RoslynSemanticModel(semanticModel));

            foreach (var analyzerMessage in analyzerMessages)
            {
                var diagnostic = ConvertAnalyzerMessageToDiagnostic(context.Node, analyzerMessage);

                if (diagnostic != null)
                    context.ReportDiagnostic(diagnostic);
            }
        }

        private static Diagnostic ConvertAnalyzerMessageToDiagnostic(SyntaxNode node, AnalyzerMessageBase analyzerMessage)
        {
            switch (analyzerMessage)
            {
                case IllegalDependencyMessage illegalDependencyMessage:
                    return CreateIllegalDependencyDiagnostic(node, illegalDependencyMessage.ToString(), illegalDependencyMessage.IssueKind);

                case TooManyIssuesMessage tooManyIssuesMessage:
                    return CreateTooManyIssuesDiagnostic(node, tooManyIssuesMessage.ToString(), tooManyIssuesMessage.IssueKind);

                case ConfigErrorMessage configErrorMessage:
                    return CreateConfigExceptionDiagnostic(node, configErrorMessage.ToString());

                default:
                    return null;
            }
        }

        private static Diagnostic CreateIllegalDependencyDiagnostic(SyntaxNode node, string message, IssueKind issueKind)
        {
            // TODO: get location from typeDependency.SourceSegment?
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message, issueKind);
        }

        private static Diagnostic CreateTooManyIssuesDiagnostic(SyntaxNode node, string message, IssueKind issueKind)
        {
            var location = Location.Create(node.SyntaxTree, node.Span);
            return CreateDiagnostic(IllegalDependencyDescriptor, location, message, issueKind);
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

        private static int GetWarningLevel(DiagnosticSeverity severity) => severity == DiagnosticSeverity.Error ? 0 : 1;

        private static IProjectFileResolver CreateProjectFileResolver()
        {
            var projectFileResolver = new WorkspaceProjectFileResolver(LogTraceMessage);
            return new CachingProjectFileResolver(projectFileResolver, new DateTimeProvider(), ProjectFileCachingTimeSpan);
        }

        private static CachingAnalyzerProvider CreateDependencyAnalyzerProvider(ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(LogTraceMessage);
            var analyzerProvider = new AnalyzerProvider(dependencyAnalyzerFactory, typeDependencyEnumerator);
            return new CachingAnalyzerProvider(analyzerProvider, new DateTimeProvider(), AnalyzerCachingTimeSpan);
        }

        private static void LogExceptionToActivityLog(Exception exception)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var vsActivityLog = VisualStudioServiceGateway.GetActivityLogService();
            vsActivityLog.LogEntry((uint) __ACTIVITYLOG_ENTRYTYPE.ALE_ERROR, ProductConstants.ToolName, exception.ToString());
        }

        protected static void LogTraceMessage(string message) => Debug.WriteLine($"[{ProductConstants.ToolName}] {message}");
    }
}

#pragma warning restore RS1022
