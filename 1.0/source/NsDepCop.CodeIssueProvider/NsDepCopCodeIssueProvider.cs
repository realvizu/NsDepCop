using Codartis.NsDepCop.Core;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using Roslyn.Services.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Codartis.NsDepCop.CodeIssueProvider
{
    [ExportCodeIssueProvider("NsDepCop.CodeIssueProvider", LanguageNames.CSharp)]
    class NsDepCopCodeIssueProvider : ICodeIssueProvider
    {
        private NsDepCopConfig _config;
        private string _configPath;
        private bool _configFileExists;
        private DateTime _configLastReadUtc;

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

        public IEnumerable<CodeIssue> GetIssues(IDocument document, CommonSyntaxNode node, CancellationToken cancellationToken)
        {
            // Find out the location of the config file.
            if (_configPath == null)
            {
                var projectFilePath = document.Project.FilePath;
                var projectFileDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('\\'));
                _configPath = Path.Combine(projectFileDirectory, Constants.DEFAULT_CONFIG_FILE_NAME);
            }

            _configFileExists = File.Exists(_configPath);

            // No config file means no analysis.
            if (!_configFileExists)
                yield break;

            // Read the config for the first time, or whenever the file changes.
            if (_config == null || File.GetLastWriteTimeUtc(_configPath) > _configLastReadUtc)
            {
                _configLastReadUtc = DateTime.UtcNow;
                _config = new NsDepCopConfig(_configPath);
            }

            // If analysis is switched off in the config file, then bail out.
            if (!_config.IsEnabled)
                yield break;

            var semanticModel = document.GetSemanticModel(cancellationToken);
            var dependencyViolation = DependencyAnalyzer.ProcessSyntaxNode(node, semanticModel, _config);
            if (dependencyViolation != null)
            {
                yield return new CodeIssue(_config.CodeIssueKind, node.Span, dependencyViolation.ToString());
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
