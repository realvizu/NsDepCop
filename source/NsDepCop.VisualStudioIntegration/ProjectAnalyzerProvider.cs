using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Creates and stores dependency analyzers for C# projects.
    /// </summary>
    internal class ProjectAnalyzerProvider : IDisposable
    {
        /// <summary>
        /// Cache for mapping source files to project files. The key is the source file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _sourceFileToProjectFileMap;

        /// <summary>
        /// Cache for mapping project files to their corresponding dependency analyzer. The key is the project file name with full path.
        /// </summary>
        private readonly ConcurrentDictionary<string, IDependencyAnalyzer> _projectFileToDependencyAnalyzerMap;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public ProjectAnalyzerProvider()
        {
            _sourceFileToProjectFileMap = new ConcurrentDictionary<string, string>();
            _projectFileToDependencyAnalyzerMap = new ConcurrentDictionary<string, IDependencyAnalyzer>();
        }

        public void Dispose()
        {
            foreach (var dependencyAnalyzer in _projectFileToDependencyAnalyzerMap.Values)
                dependencyAnalyzer.Dispose();
        }

        /// <summary>
        /// Retrieves an up-to-date analyzer for a source file + assembly name pair.
        /// </summary>
        /// <param name="sourceFilePath">The full path of a source file.</param>
        /// <param name="assemblyName">The name of the assembly that the source file belongs to.</param>
        /// <returns>A ProjectAnalyzer, or null if cannot be retrieved.</returns>
        public IDependencyAnalyzer GetDependencyAnalyzer(string sourceFilePath, string assemblyName)
        {
            var projectFilePath = GetProjectFilePath(sourceFilePath, assemblyName);
            if (projectFilePath == null )
                return null;

            var dependencyAnalyzer = GetDependencyAnalyzer(projectFilePath);
            if (dependencyAnalyzer.ConfigState != ConfigState.Enabled)
                return null;

            return dependencyAnalyzer;
        }

        private IDependencyAnalyzer GetDependencyAnalyzer(string projectFilePath)
        {
            bool added;
            var dependencyAnalyzer = _projectFileToDependencyAnalyzerMap.GetOrAdd(projectFilePath, CreateDependencyAnalyzer, out added);

            if (!added)
                dependencyAnalyzer.RefreshConfig();

            return dependencyAnalyzer;
        }

        private static IDependencyAnalyzer CreateDependencyAnalyzer(string projectFilePath)
        {
            var configFileName = CreateConfigFileName(projectFilePath);
            return DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName, Parsers.Roslyn);
        }

        private static string CreateConfigFileName(string projectFilePath)
        {
            var projectFileDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('\\'));
            return Path.Combine(projectFileDirectory, ProductConstants.DefaultConfigFileName);
        }

        private string GetProjectFilePath(string sourceFilePath, string assemblyName)
        {
            return _sourceFileToProjectFileMap.GetOrAdd(sourceFilePath, i => FindProjectFile(i, assemblyName));
        }

        private static string FindProjectFile(string sourceFilePath, string assemblyName)
        {
            try
            {
                var directoryPath = Path.GetDirectoryName(sourceFilePath);

                while (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    var candidateProjectFiles = Directory.GetFiles(directoryPath, "*.csproj", SearchOption.TopDirectoryOnly);
                    foreach (var projectFilePath in candidateProjectFiles)
                    {
                        if (IsProjectFileForAssembly(projectFilePath, assemblyName))
                            return projectFilePath;
                    }

                    var parentDirectory = Directory.GetParent(directoryPath);
                    directoryPath = parentDirectory?.FullName;
                }

                return null;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Exception in FindProjectFile({sourceFilePath}, {assemblyName}): {e}");
                return null;
            }
        }

        private static bool IsProjectFileForAssembly(string projectFilePath, string assemblyName)
        {
            try
            {
                using (var stream = new FileStream(projectFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var xDocument = XDocument.Load(stream);

                    var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
                    xmlNamespaceManager.AddNamespace("x", @"http://schemas.microsoft.com/developer/msbuild/2003");
                    var projectAssemblyName = xDocument.XPathSelectElement("/x:Project/x:PropertyGroup/x:AssemblyName", xmlNamespaceManager)?.Value;
                    if (projectAssemblyName == null)
                        return false;

                    return string.Equals(projectAssemblyName.Trim(), assemblyName.Trim(), StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Exception in IsProjectFileForAssembly({projectFilePath}, {assemblyName}): {e}");
                return false;
            }
        }
    }
}
