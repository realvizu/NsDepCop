using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Implementation.Roslyn;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Creates and stores dependency analyzers for C# projects.
    /// </summary>
    internal static class ProjectDependencyAnalyzerRepository
    {
        /// <summary>
        /// Cache for mapping source files to project files. The key is the source file name with full path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> SourceFileToProjectFileMap =
            new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Cache for mapping project files to their analyzers. The key is the project file name with full path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, DependencyAnalyzer> ProjectFileToAnalyzerMap =
            new ConcurrentDictionary<string, DependencyAnalyzer>();

        /// <summary>
        /// Retrieves an up-to-date analyzer for a source file + assembly name pair.
        /// </summary>
        /// <param name="sourceFilePath">The full path of a source file.</param>
        /// <param name="assemblyName">The name of the assembly that the source file belongs to.</param>
        /// <returns>A ProjectAnalyzer, or null if cannot be retrieved.</returns>
        public static DependencyAnalyzer GetAnalyzer(string sourceFilePath, string assemblyName)
        {
            var projectFilePath = GetProjectFilePath(sourceFilePath, assemblyName);
            return projectFilePath == null 
                ? null 
                : GetProjectAnalyzer(projectFilePath);
        }

        private static DependencyAnalyzer GetProjectAnalyzer(string projectFilePath)
        {
            var analyzerCreatedNow = false;
            var projectAnalyzer = ProjectFileToAnalyzerMap.GetOrAdd(projectFilePath,
                i =>
                {
                    analyzerCreatedNow = true;
                    return CreateAnalyzer(i);
                });

            if (!analyzerCreatedNow)
                projectAnalyzer.RefreshConfig();

            return projectAnalyzer;
        }

        private static DependencyAnalyzer CreateAnalyzer(string projectFilePath)
        {
            var configFileName = CreateConfigFileName(projectFilePath);
            return DependencyAnalyzerFactory.Create(ParserType.Roslyn, configFileName) as DependencyAnalyzer;
        }

        private static string CreateConfigFileName(string projectFilePath)
        {
            var projectFileDirectory = projectFilePath.Substring(0, projectFilePath.LastIndexOf('\\'));
            return Path.Combine(projectFileDirectory, Constants.DEFAULT_CONFIG_FILE_NAME);
        }

        private static string GetProjectFilePath(string sourceFilePath, string assemblyName)
        {
            return SourceFileToProjectFileMap.GetOrAdd(sourceFilePath, i => FindProjectFile(i, assemblyName));
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
