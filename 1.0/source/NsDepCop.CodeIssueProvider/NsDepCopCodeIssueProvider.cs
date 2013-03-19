using Codartis.NsDepCop.Core;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Codartis.NsDepCop.CodeIssueProvider
{
    /// <summary>
    /// Implements a CodeIssueProvider that returns NsDepCop issues 
    /// about the source code loaded into the Visual Studio code editor.
    /// </summary>
    [ExportCodeIssueProvider("NsDepCop.CodeIssueProvider", LanguageNames.CSharp)]
    class NsDepCopCodeIssueProvider : ICodeIssueProvider
    {
        /// <summary>
        /// Cache for config handlers of C# projects. The key is the project file path.
        /// </summary>
        private Dictionary<string, ConfigHandler> _configHandlers = new Dictionary<string, ConfigHandler>();

        /// <summary>
        /// Indicates that a config exception was already reported. To avoid multiple error reports.
        /// </summary>
        private bool _configExceptionAlreadyReported = false;

        /// <summary>
        /// Gets the syntax node types that this code issue provider will be invoked for.
        /// </summary>
        /// <returns>A collection of SyntaxNode descendant types.</returns>
        public IEnumerable<Type> SyntaxNodeTypes
        {
            get
            {
                yield return typeof(IdentifierNameSyntax);
                yield return typeof(InvocationExpressionSyntax);
                yield return typeof(ElementAccessExpressionSyntax);
                yield return typeof(QueryExpressionSyntax);
                yield return typeof(PredefinedTypeSyntax);
                yield return typeof(LiteralExpressionSyntax);
                yield return typeof(NullableTypeSyntax);
                yield return typeof(AliasQualifiedNameSyntax);
            }
        }

        /// <summary>
        /// Analyzes a syntax node a returns code issues if necessary.
        /// </summary>
        /// <param name="document">The document in the code editor.</param>
        /// <param name="node">The syntax node to be analyzed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Any number of CodeIssues (including none).</returns>
        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            // If something is missing from the necessary input then bail out.
            if (node == null || document == null || document.Project == null || document.Project.FilePath == null)
                yield break;

            // The project file path is the key for finding the right NsDepCop config handler.
            var projectFilePath = document.Project.FilePath;

            // Get an NsDepCop config handler instance for the current project (get from cache or create a new one).
            ConfigHandler configHandler;
            if (!_configHandlers.TryGetValue(projectFilePath, out configHandler))
            {
                configHandler = new ConfigHandler(projectFilePath);
                _configHandlers.Add(projectFilePath, configHandler);
            }

            // Get the NsDepCop config. It can throw if the config file is malformed.
            NsDepCopConfig config = null;
            Exception configException = null;
            try
            {
                config = configHandler.GetConfig();
                _configExceptionAlreadyReported = false;
            }
            catch (Exception e)
            {
                configException = e;
            }

            // Report config exception if there's one. Can't do it inside the catch block because of a C# limitation.
            if (configException != null && !_configExceptionAlreadyReported)
            {
                _configExceptionAlreadyReported = true;
                var message = string.Format("Error loading NsDepCop config: {0}", configException.Message);
                yield return new CodeIssue(CodeIssueKind.Error, node.Span, message);
            }

            if (config == null)
                yield break;

            // If analysis is switched off in the config file, then bail out.
            if (!config.IsEnabled)
                yield break;

            // Analyze this node and return CodeIssue if needed.
            var semanticModel = document.GetSemanticModel(cancellationToken);
            var dependencyViolation = DependencyAnalyzer.ProcessSyntaxNode(node, semanticModel, config);
            if (dependencyViolation != null)
            {
                yield return new CodeIssue(config.CodeIssueKind, node.Span, dependencyViolation.ToString());
            }
        }

        #region Unimplemented ICodeIssueProvider members

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxToken token, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> SyntaxTokenKinds
        {
            get
            {
                return null;
            }
        }

        #endregion
    }
}
