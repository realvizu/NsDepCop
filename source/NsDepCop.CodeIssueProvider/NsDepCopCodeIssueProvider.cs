using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Immutable;
using System.IO;

namespace Codartis.NsDepCop.CodeIssueProvider
{
    /// <summary>
    /// Implements a DiagnosticAnalyzer that returns NsDepCop issues 
    /// about the source code loaded into the Visual Studio code editor.
    /// </summary>
    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer("NsDepCop.CodeIssueProvider", LanguageNames.CSharp)]
    public class NsDepCopCodeIssueProvider : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        /// <summary>
        /// Cache for mapping project files to config handlers. The key is the config file name with full path.
        /// </summary>
        private Dictionary<string, ConfigHandler> _projectFileToConfigHandlerMap = new Dictionary<string, ConfigHandler>();

        /// <summary>
        /// Cache for mapping source files to project files. The key is the source file name with full path.
        /// </summary>
        private Dictionary<string, string> _sourceFileToProjectFileMap = new Dictionary<string, string>();

        /// <summary>
        /// Indicates that a config exception was already reported. To avoid multiple error reports.
        /// </summary>
        private bool _configExceptionAlreadyReported = false;

        /// <summary>
        /// Descriptor for the 'Illegal namespace dependency' diagnostic.
        /// </summary>
        private DiagnosticDescriptor DiagnosticDescriptorForIllegalNsDep = new DiagnosticDescriptor(
            Constants.DIAGNOSTIC_ID_ILLEGAL_NS_DEP,
            Constants.DIAGNOSTIC_DESC_ILLEGAL_NS_DEP,
            Constants.DIAGNOSTIC_FORMAT_ILLEGAL_NS_DEP,
            Constants.TOOL_NAME,
            DiagnosticSeverity.Error);

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private DiagnosticDescriptor DiagnosticDescriptorForConfigException = new DiagnosticDescriptor(
            Constants.DIAGNOSTIC_ID_CONFIG_EXCEPTION,
            Constants.DIAGNOSTIC_DESC_CONFIG_EXCEPTION,
            Constants.DIAGNOSTIC_FORMAT_CONFIG_EXCEPTION,
            Constants.TOOL_NAME,
            DiagnosticSeverity.Error);

        ImmutableArray<DiagnosticDescriptor> IDiagnosticAnalyzer.SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    DiagnosticDescriptorForIllegalNsDep, 
                    DiagnosticDescriptorForConfigException);
            }
        }

        ImmutableArray<SyntaxKind> ISyntaxNodeAnalyzer<SyntaxKind>.SyntaxKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(
                    SyntaxKind.IdentifierName,
                    SyntaxKind.GenericName);
            }
        }

        /// <summary>
        /// Called for each node whose language-specific kind is an element of SyntaxKindsOfInterest.
        /// </summary>
        /// <param name="node">A node of a kind of interest</param>
        /// <param name="semanticModel">A SemanticModel for the compilation unit</param>
        /// <param name="addDiagnostic">A delegate to be used to emit diagnostics</param>
        /// <param name="cancellationToken">A token for cancelling the computation</param>
        void ISyntaxNodeAnalyzer<SyntaxKind>.AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        {
            // The project file path is the key for finding the right NsDepCop config handler.
            var projectFilePath = FindProjectFile(node.SyntaxTree.FilePath);

            // Get an NsDepCop config handler instance for the current project (get from cache or create a new one).
            ConfigHandler configHandler;
            if (!_projectFileToConfigHandlerMap.TryGetValue(projectFilePath, out configHandler))
            {
                configHandler = new ConfigHandler(projectFilePath);
                _projectFileToConfigHandlerMap.Add(projectFilePath, configHandler);
            }

            // Get the NsDepCop config. It can throw if the config file is malformed.
            NsDepCopConfig config = null;
            try
            {
                config = configHandler.GetConfig();
                _configExceptionAlreadyReported = false;
            }
            catch (Exception configException)
            {
                // Report config exception.
                if (!_configExceptionAlreadyReported)
                {
                    _configExceptionAlreadyReported = true;
                    addDiagnostic(Diagnostic.Create(DiagnosticDescriptorForConfigException, Location.None, configException.Message));
                }
            }

            if (config == null)
                return;

            // If analysis is switched off in the config file, then bail out.
            if (!config.IsEnabled)
                return;

            var dependencyViolation = SyntaxNodeAnalyzer.Analyze(node, semanticModel, config);
            if (dependencyViolation != null)
            {
                addDiagnostic(CreateIllegalNsDepDiagnostic(node, dependencyViolation, config.IssueKind));
            }
        }

        private Diagnostic CreateIllegalNsDepDiagnostic(SyntaxNode node, DependencyViolation dependencyViolation, IssueKind issueKind)
        {
            var message = string.Format(DiagnosticDescriptorForIllegalNsDep.MessageFormat,
                dependencyViolation.IllegalDependency.From,
                dependencyViolation.IllegalDependency.To,
                dependencyViolation.ReferencingTypeName,
                dependencyViolation.SourceSegment.Text,
                dependencyViolation.ReferencedTypeName);

            var severity = issueKind.ToDiagnosticSeverity();

            var warningLevel = severity == DiagnosticSeverity.Warning ? 1 : 0;

            return Diagnostic.Create(
                DiagnosticDescriptorForIllegalNsDep.Id,
                DiagnosticDescriptorForIllegalNsDep.Category,
                message,
                severity,
                warningLevel,
                false,
                Location.Create(node.SyntaxTree, node.Span));
        }

        private string FindProjectFile(string sourceFilePath)
        {
            string projectFilePath;
            if (_sourceFileToProjectFileMap.TryGetValue(sourceFilePath, out projectFilePath))
                return projectFilePath;

            var directoryPath = sourceFilePath == null ? null : Path.GetDirectoryName(sourceFilePath);

            while (!string.IsNullOrEmpty(directoryPath))
            {
                projectFilePath = Directory.GetFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (projectFilePath != null)
                {
                    _sourceFileToProjectFileMap.Add(sourceFilePath, projectFilePath);
                    return projectFilePath;
                }

                var parentDirectory = Directory.GetParent(directoryPath);
                directoryPath = parentDirectory == null ? null : parentDirectory.FullName;
            }

            return null;
        }
    }
}
