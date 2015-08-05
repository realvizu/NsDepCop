using System;
using System.IO;
using Codartis.NsDepCop.Core.Common;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Holds analyzer config info for a certain C# project file.
    /// Can load info from an NsDepCop config file and can also refresh it if the file was updated since last read.
    /// </summary>
    internal class ProjectAnalyzerConfig
    {
        private readonly string _projectFilePath;

        private string _configPath;
        private bool _configFileExists;
        private DateTime _configLastReadUtc;
        private Exception _configException;
        private NsDepCopConfig _config;
        private DependencyValidator _dependencyValidator;

        public ProjectAnalyzerConfig(string projectFilePath)
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

        public DependencyValidator DependencyValidator
        {
            get { return _dependencyValidator; }
        }

        public Exception ConfigException
        {
            get { return _configException; }
        }

        /// <summary>
        /// Loads or refreshes NsDepCop config info from file.
        /// </summary>
        public void Refresh()
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
