using System;
using System.Collections.Concurrent;
using System.IO;
using Codartis.NsDepCop.Analysis;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    /// <summary>
    /// Creates and stores dependency analyzers for C# projects.
    /// Ensures that the analyzers' configs are always refreshed.
    /// </summary>
    public sealed class AnalyzerProvider : IAnalyzerProvider
    {
        private readonly IDependencyAnalyzerFactory _dependencyAnalyzerFactory;
        private readonly IAssemblyDependencyAnalyzerFactory _assemblyDependencyAnalyzerFactory;
        private readonly IConfigProviderFactory _configProviderFactory;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;

        /// <summary>
        /// Maps project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IDependencyAnalyzer> _projectFileToDependencyAnalyzerMap = new();
        private readonly ConcurrentDictionary<string, IAssemblyDependencyAnalyzer> _projectFileToAssemblyDependencyAnalyzerMap = new();

        public AnalyzerProvider(
            IDependencyAnalyzerFactory dependencyAnalyzerFactory,
            IAssemblyDependencyAnalyzerFactory assemblyDependencyAnalyzerFactory,
            IConfigProviderFactory configProviderFactory,
            ITypeDependencyEnumerator typeDependencyEnumerator)
        {
            _dependencyAnalyzerFactory = dependencyAnalyzerFactory ?? throw new ArgumentNullException(nameof(dependencyAnalyzerFactory));
            _assemblyDependencyAnalyzerFactory = assemblyDependencyAnalyzerFactory ?? throw new ArgumentNullException(nameof(assemblyDependencyAnalyzerFactory));
            _configProviderFactory = configProviderFactory ?? throw new ArgumentNullException(nameof(configProviderFactory));
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
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
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(projectFileDirectory);
            return _dependencyAnalyzerFactory.Create(configProvider, _typeDependencyEnumerator);
        }

        public IAssemblyDependencyAnalyzer GetAssemblyDependencyAnalyzer(string csprojFilePath)
        {
            if (string.IsNullOrWhiteSpace(csprojFilePath))
                throw new ArgumentException("Filename must not be null or whitespace.", nameof(csprojFilePath));

            var dependencyAnalyzer = _projectFileToAssemblyDependencyAnalyzerMap.GetOrAdd(csprojFilePath, CreateAssemblyDependencyAnalyzer, out var added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private IAssemblyDependencyAnalyzer CreateAssemblyDependencyAnalyzer(string projectFilePath)
        {
            var projectFileDirectory = Path.GetDirectoryName(projectFilePath);
            var configProvider = _configProviderFactory.CreateFromMultiLevelXmlConfigFile(projectFileDirectory);
            return _assemblyDependencyAnalyzerFactory.Create(configProvider);
        }
    }
}