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
        private readonly DiagnosticDescriptor _diagnosticDescriptorForIllegalNsDep = new DiagnosticDescriptor(
            Constants.DIAGNOSTIC_ILLEGALDEP_ID,
            Constants.DIAGNOSTIC_ILLEGALDEP_DESC,
            Constants.DIAGNOSTIC_ILLEGALDEP_FORMAT,
            Constants.TOOL_NAME,
            Constants.DIAGNOSTIC_ILLEGALDEP_DEFAULTSEVERITY.ToDiagnosticSeverity(),
            isEnabledByDefault: true);

        /// <summary>
        /// Descriptor for the 'Config exception' diagnostic.
        /// </summary>
        private readonly DiagnosticDescriptor _diagnosticDescriptorForConfigException = new DiagnosticDescriptor(
            Constants.DIAGNOSTIC_CONFIGEXCEPTION_ID,
            Constants.DIAGNOSTIC_CONFIGEXCEPTION_DESC,
            Constants.DIAGNOSTIC_CONFIGEXCEPTION_FORMAT,
            Constants.TOOL_NAME,
            Constants.DIAGNOSTIC_CONFIGEXCEPTION_DEFAULTSEVERITY.ToDiagnosticSeverity(),
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    _diagnosticDescriptorForIllegalNsDep,
                    _diagnosticDescriptorForConfigException);
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
            catch (Exception configException)
            {
                // Report config exception.
                if (!_configExceptionAlreadyReported)
                {
                    _configExceptionAlreadyReported = true;
                    context.ReportDiagnostic(Diagnostic.Create(_diagnosticDescriptorForConfigException, Location.None, configException.Message));
                }
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
            var message = string.Format(_diagnosticDescriptorForIllegalNsDep.MessageFormat.ToString(),
                dependencyViolation.IllegalDependency.From,
                dependencyViolation.IllegalDependency.To,
                dependencyViolation.ReferencingTypeName,
                dependencyViolation.SourceSegment.Text,
                dependencyViolation.ReferencedTypeName);

            var severity = issueKind.ToDiagnosticSeverity();

            var warningLevel = severity == DiagnosticSeverity.Error ? 0 : 1;

            return Diagnostic.Create(
                _diagnosticDescriptorForIllegalNsDep.Id,
                _diagnosticDescriptorForIllegalNsDep.Category,
                message,
                severity: severity,
                defaultSeverity: Constants.DIAGNOSTIC_ILLEGALDEP_DEFAULTSEVERITY.ToDiagnosticSeverity(),
                isEnabledByDefault: true,
                warningLevel: warningLevel,
                location: Location.Create(node.SyntaxTree, node.Span));
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