using System;
using System.Collections.Generic;
using System.IO;
using Codartis.NsDepCop.Core.Analyzer.Roslyn;
using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Namespace dependency analyzer for a certain C# project file.
    /// Can load config info from an NsDepCop config file and can also refresh it if the file was updated since last read.
    /// </summary>
    internal class ProjectAnalyzer
    {
        private readonly string _projectFilePath;

        private string _configPath;
        private bool _configFileExists;
        private DateTime _configLastReadUtc;
        private Exception _configException;
        private NsDepCopConfig _config;
        private DependencyValidator _dependencyValidator;

        public ProjectAnalyzer(string projectFilePath)
        {
            _projectFilePath = projectFilePath;
        }

        private bool IsConfigLoaded
        {
            get { return _config != null; }
        }

        public ProjectAnalyzerState State
        {
            get
            {
                if (_configFileExists && IsConfigLoaded && _config.IsEnabled)
                    return ProjectAnalyzerState.Enabled;

                if (_configFileExists && !IsConfigLoaded && _configException != null)
                    return ProjectAnalyzerState.ConfigError;

                if (!_configFileExists || (IsConfigLoaded && !_config.IsEnabled))
                    return ProjectAnalyzerState.Disabled;

                throw new Exception("Inconsitent ProjectAnalyzerConfig state.");
            }
        }

        public IssueKind IssueKind
        {
            get { return IsConfigLoaded ? _config.IssueKind : IssueKind.Error; }
        }

        public Exception ConfigException
        {
            get { return _configException; }
        }

        /// <summary>
        /// Performs namespace dependency analysis on a syntax node and returns the resulting violations.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="semanticModel">The semantic model of the compilation.</param>
        /// <returns>A collection of dependency violations or empty collection if found none.</returns>
        public IEnumerable<DependencyViolation> AnalyzeNode(SyntaxNode node, SemanticModel semanticModel)
        {
            return SyntaxNodeAnalyzer.Analyze(node, semanticModel, _dependencyValidator);
        }

        /// <summary>
        /// Loads or refreshes NsDepCop config info from file.
        /// </summary>
        public void RefreshConfig()
        {
            if (_configPath == null)
                _configPath = CreateConfigFilePath(_projectFilePath);

            _configFileExists = File.Exists(_configPath);

            if (!_configFileExists)
            {
                _configException = null;
                _config = null;
                _dependencyValidator = null;
                return;
            }

            try
            {
                // Read the config if it was never read, or whenever the file changes.
                if (!IsConfigLoaded || _configLastReadUtc < File.GetLastWriteTimeUtc(_configPath))
                {
                    _configLastReadUtc = DateTime.UtcNow;

                    _configException = null;
                    _config = new NsDepCopConfig(_configPath);
                    _dependencyValidator = new DependencyValidator(
                        _config.AllowedDependencies,
                        _config.DisallowedDependencies,
                        _config.ChildCanDependOnParentImplicitly);
                }
            }
            catch (Exception e)
            {
                _configException = e;
                _config = null;
                _dependencyValidator = null;
            }
        }

        private static string CreateConfigFilePath(string projectFilePath)
        {
            var projectFileDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('\\'));
            return Path.Combine(projectFileDirectory, Constants.DEFAULT_CONFIG_FILE_NAME);
        }
    }
}
