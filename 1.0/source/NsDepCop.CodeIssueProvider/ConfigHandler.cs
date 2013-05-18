using Codartis.NsDepCop.Core;
using System;
using System.IO;

namespace Codartis.NsDepCop.CodeIssueProvider
{
    /// <summary>
    /// Reads and stores NsDepConfig info for a given C# project.
    /// </summary>
    internal class ConfigHandler
    {
        private string _projectFilePath;
        private NsDepCopConfig _config;
        private string _configPath;
        private bool _configFileExists;
        private DateTime _configLastReadUtc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="projectFilePath">The full path of the C# project file.</param>
        public ConfigHandler(string projectFilePath)
        {
            _projectFilePath = projectFilePath;
        }

        /// <summary>
        /// Returns an up-to-date NsDepCop config object for this project if one exists.
        /// </summary>
        /// <returns>An NsDepCop config object or null if not exists.</returns>
        public NsDepCopConfig GetConfig()
        {
            // Find out the location of the config file.
            if (_configPath == null)
            {
                var projectFileDirectory = _projectFilePath.Substring(0, _projectFilePath.LastIndexOf('\\'));
                _configPath = Path.Combine(projectFileDirectory, Constants.DEFAULT_CONFIG_FILE_NAME);
            }

            _configFileExists = File.Exists(_configPath);

            // No config file means no analysis.
            if (!_configFileExists)
                return null;

            // Read the config for the first time, or whenever the file changes.
            if (_config == null || File.GetLastWriteTimeUtc(_configPath) > _configLastReadUtc)
            {
                _configLastReadUtc = DateTime.UtcNow;
                _config = new NsDepCopConfig(_configPath);
            }

            return _config;
        }
    }
}
