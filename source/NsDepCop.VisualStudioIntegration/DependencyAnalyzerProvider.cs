using System;
using System.Collections.Concurrent;
using System.IO;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Creates and stores dependency analyzers for C# projects.
    /// Ensures that the analyzers' configs are always refreshed.
    /// </summary>
    internal class DependencyAnalyzerProvider : IDependencyAnalyzerProvider
    {
        private readonly IDependencyAnalyzerFactory _dependencyAnalyzerFactory;
        private readonly Action<string> _diagnosticMessageHandler;

        /// <summary>
        /// Maps project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IDependencyAnalyzer> _projectFileToDependencyAnalyzerMap;

        public DependencyAnalyzerProvider(IDependencyAnalyzerFactory dependencyAnalyzerFactory, Action<string> diagnosticMessageHandler = null)
        {
            if (dependencyAnalyzerFactory == null)
                throw new ArgumentNullException(nameof(dependencyAnalyzerFactory));

            _dependencyAnalyzerFactory = dependencyAnalyzerFactory;
            _diagnosticMessageHandler = diagnosticMessageHandler;
            _projectFileToDependencyAnalyzerMap = new ConcurrentDictionary<string, IDependencyAnalyzer>();
        }

        public void Dispose()
        {
            foreach (var dependencyAnalyzer in _projectFileToDependencyAnalyzerMap.Values)
                dependencyAnalyzer.Dispose();
        }

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath)
        {
            if (string.IsNullOrWhiteSpace(csprojFilePath))
                throw new ArgumentException("Filename must not be null or whitespace.", nameof(csprojFilePath));

            bool added;
            var dependencyAnalyzer = _projectFileToDependencyAnalyzerMap.GetOrAdd(csprojFilePath, CreateDependencyAnalyzer, out added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private IDependencyAnalyzer CreateDependencyAnalyzer(string projectFilePath)
        {
            var configFileName = CreateConfigFileName(projectFilePath);
            return _dependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName, Parsers.Roslyn, _diagnosticMessageHandler);
        }

        private static string CreateConfigFileName(string projectFilePath)
        {
            var projectFileDirectory = Path.GetDirectoryName(projectFilePath);
            if (projectFileDirectory == null)
                throw new Exception($"Can not determine directory from full path '{projectFilePath}'");

            return Path.Combine(projectFileDirectory, ProductConstants.DefaultConfigFileName);
        }
    }
}
