using System;
using System.Collections.Concurrent;
using System.IO;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Creates and stores dependency analyzers for C# projects.
    /// Ensures that the analyzers' configs are always refreshed.
    /// </summary>
    public class AnalyzerProvider : IAnalyzerProvider
    {
        private readonly IDependencyAnalyzerFactory _dependencyAnalyzerFactory;

        /// <summary>
        /// Maps project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IRefreshableDependencyAnalyzer> _projectFileToDependencyAnalyzerMap;

        public AnalyzerProvider(IDependencyAnalyzerFactory dependencyAnalyzerFactory)
        {
            _dependencyAnalyzerFactory = dependencyAnalyzerFactory ?? throw new ArgumentNullException(nameof(dependencyAnalyzerFactory));
            _projectFileToDependencyAnalyzerMap = new ConcurrentDictionary<string, IRefreshableDependencyAnalyzer>();
        }

        public void Dispose()
        {
            foreach (var dependencyAnalyzer in _projectFileToDependencyAnalyzerMap.Values)
                if (dependencyAnalyzer is IDisposable disposable)
                    disposable.Dispose();
        }

        public IRefreshableDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath)
        {
            if (string.IsNullOrWhiteSpace(csprojFilePath))
                throw new ArgumentException("Filename must not be null or whitespace.", nameof(csprojFilePath));

            bool added;
            var dependencyAnalyzer = _projectFileToDependencyAnalyzerMap.GetOrAdd(csprojFilePath, CreateDependencyAnalyzer, out added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private IRefreshableDependencyAnalyzer CreateDependencyAnalyzer(string projectFilePath)
        {
            var projectFileDirectory = Path.GetDirectoryName(projectFilePath);
            return _dependencyAnalyzerFactory.CreateFromMultiLevelXmlConfigFile(projectFileDirectory);
        }
    }
}
