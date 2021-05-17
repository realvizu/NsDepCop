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
    public sealed class AnalyzerProvider : IAnalyzerProvider
    {
        private readonly IDependencyAnalyzerFactory _dependencyAnalyzerFactory;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;

        /// <summary>
        /// Maps project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IDependencyAnalyzer> _projectFileToDependencyAnalyzerMap;

        public AnalyzerProvider(IDependencyAnalyzerFactory dependencyAnalyzerFactory, ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            _dependencyAnalyzerFactory = dependencyAnalyzerFactory ?? throw new ArgumentNullException(nameof(dependencyAnalyzerFactory));
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _projectFileToDependencyAnalyzerMap = new ConcurrentDictionary<string, IDependencyAnalyzer>();
        }

        public void Dispose()
        {
            foreach (var dependencyAnalyzer in _projectFileToDependencyAnalyzerMap.Values)
                if (dependencyAnalyzer is IDisposable disposable)
                    disposable.Dispose();
        }

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath)
        {
            if (string.IsNullOrWhiteSpace(csprojFilePath))
                throw new ArgumentException("Filename must not be null or whitespace.", nameof(csprojFilePath));

            var dependencyAnalyzer = _projectFileToDependencyAnalyzerMap.GetOrAdd(csprojFilePath, CreateDependencyAnalyzer, out var added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private IDependencyAnalyzer CreateDependencyAnalyzer(string projectFilePath)
        {
            var projectFileDirectory = Path.GetDirectoryName(projectFilePath);
            return _dependencyAnalyzerFactory.Create(projectFileDirectory, _typeDependencyEnumerator);
        }
    }
}
