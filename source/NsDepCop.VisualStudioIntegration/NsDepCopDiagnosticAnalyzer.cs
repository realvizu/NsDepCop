using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NsDepCopDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Cache for mapping project files to config handlers. The key is the config file name with full path.
        /// </summary>
        private readonly Dictionary<string, ConfigHandler> _projectFileToConfigHandlerMap = new Dictionary<string, ConfigHandler>();

        /// <summary>
        /// Cache for mapping source files to project files. The key is the source file name with full path.
        /// </summary>
        private readonly Dictionary<string, string> _sourceFileToProjectFileMap = new Dictionary<string, string>();

        /// <summary>
        /// Indicates that a config exception was already reported. To avoid multiple error reports.
        /// </summary>
        private bool _configExceptionAlreadyReported;

        /// <summary>
        /// Descriptor for the 'Illegal namespace dependency' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor _illegalDependencyDescriptor = Constants.IllegalDependencyIssue.ToDiagnosticDescriptor();

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private static readonly DiagnosticDescriptor _configExceptionDescriptor = Constants.ConfigExceptionIssue.ToDiagnosticDescriptor();

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
            //The project file path is the key for finding the right NsDepCop config handler.
            var projectFilePath = FindProjectFile(context);
            if (projectFilePath == null)
                return;

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
            catch (Exception exception)
            {
                ReportConfigException(context, exception);
            }

            if (config == null)
                return;

            // If analysis is switched off in the config file, then bail out.
            if (!config.IsEnabled)
                return;

            var dependencyValidator = configHandler.GetDependencyValidator();
            var dependencyViolations = SyntaxNodeAnalyzer.Analyze(context.Node, context.SemanticModel, dependencyValidator);

            foreach (var dependencyViolation in dependencyViolations)
                context.ReportDiagnostic(CreateIllegalNsDepDiagnostic(context.Node, dependencyViolation, config.IssueKind));
        }

        private Diagnostic CreateIllegalNsDepDiagnostic(SyntaxNode node, DependencyViolation dependencyViolation, IssueKind issueKind)
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
                location: Location.Create(node.SyntaxTree, node.Span));
        }

        private Diagnostic CreateConfigExceptionDiagnostic(SyntaxNode node, Exception exception)
        {
            // We have to use a dummy location because of this Roslyn 1.0 limitation: 
            // https://github.com/dotnet/roslyn/issues/3748#issuecomment-117231706
            var location = Location.Create(node.SyntaxTree, node.Span);
            return Diagnostic.Create(_configExceptionDescriptor, location, exception.Message);
        }

        private void ReportConfigException(SyntaxNodeAnalysisContext context, Exception exception)
        {
            if (!_configExceptionAlreadyReported)
            {
                _configExceptionAlreadyReported = true;
                var diagnostic = CreateConfigExceptionDiagnostic(context.Node, exception);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private string FindProjectFile(SyntaxNodeAnalysisContext context)
        {
            if (context.Node == null ||
                context.Node.SyntaxTree == null ||
                string.IsNullOrWhiteSpace(context.Node.SyntaxTree.FilePath))
                return null;

            var sourceFilePath = context.Node.SyntaxTree.FilePath;

            string projectFilePath;
            if (_sourceFileToProjectFileMap.TryGetValue(sourceFilePath, out projectFilePath))
                return projectFilePath;

            if (context.SemanticModel == null ||
                context.SemanticModel.Compilation == null ||
                context.SemanticModel.Compilation.AssemblyName == null)
                return null;

            var assemblyName = context.SemanticModel.Compilation.AssemblyName;

            projectFilePath = FindProjectFile(sourceFilePath, assemblyName);

            _sourceFileToProjectFileMap.Add(sourceFilePath, projectFilePath);
            return projectFilePath;
        }

        private string FindProjectFile(string sourceFilePath, string assemblyName)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(sourceFilePath);

                while (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    var candidateProjectFiles = Directory.GetFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly);
                    foreach (var projectFilePath in candidateProjectFiles)
                    {
                        if (IsProjectFileForAssembly(projectFilePath, assemblyName))
                            return projectFilePath;
                    }

                    var parentDirectory = Directory.GetParent(directoryPath);
                    directoryPath = parentDirectory == null ? null : parentDirectory.FullName;
                }

                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in FindProjectFile({0}, {1}): {2}", sourceFilePath, assemblyName, e);
                return null;
            }
        }

        private bool IsProjectFileForAssembly(string projectFilePath, string assemblyName)
        {
            try
            {
                using (var stream = new FileStream(projectFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var xDocument = XDocument.Load(stream);

                    var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                    xmlNamespaceManager.AddNamespace("x", @"http://schemas.microsoft.com/developer/msbuild/2003");
                    var projectAssemblyName = xDocument.XPathSelectElement("/x:Project/x:PropertyGroup/x:AssemblyName", xmlNamespaceManager)?.Value;
                    if (projectAssemblyName == null)
                        return false;

                    return string.Equals(projectAssemblyName.Trim(), assemblyName.Trim(), StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception in IsProjectFileForAssembly({0}, {1}): {2}", projectFilePath, assemblyName, e);
                return false;
            }
        }
    }
}