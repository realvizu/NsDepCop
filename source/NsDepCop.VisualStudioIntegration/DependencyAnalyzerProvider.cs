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
        /// <summary>
        /// Maps project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IDependencyAnalyzer> _projectFileToDependencyAnalyzerMap;

        /// <summary>
        /// Callback for emitting diagnostic messages;
        /// </summary>
        private readonly Action<string> _diagnosticMessageHandler;

        public DependencyAnalyzerProvider(Action<string> diagnosticMessageHandler = null)
        {
            _projectFileToDependencyAnalyzerMap = new ConcurrentDictionary<string, IDependencyAnalyzer>();
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public void Dispose()
        {
            foreach (var dependencyAnalyzer in _projectFileToDependencyAnalyzerMap.Values)
                dependencyAnalyzer.Dispose();
        }

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath)
        {
            bool added;
            var dependencyAnalyzer = _projectFileToDependencyAnalyzerMap.GetOrAdd(csprojFilePath, CreateDependencyAnalyzer, out added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private IDependencyAnalyzer CreateDependencyAnalyzer(string projectFilePath)
        {
            var configFileName = CreateConfigFileName(projectFilePath);
            return  DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName, Parsers.Roslyn, _diagnosticMessageHandler);
        }

        private static string CreateConfigFileName(string projectFilePath)
        {
            var projectFileDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('\\'));
            return Path.Combine(projectFileDirectory, ProductConstants.DefaultConfigFileName);
        }
    }
}
