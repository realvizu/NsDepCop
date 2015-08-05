using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Creates and stores project analyzers.
    /// </summary>
    internal static class ProjectAnalyzerRepository
    {
        /// <summary>
        /// Cache for mapping source files to project files. The key is the source file name with full path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _sourceFileToProjectFileMap =
            new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Cache for mapping project files to their analyzers. The key is the project file name with full path.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ProjectAnalyzer> _projectFileToAnalyzerMap =
            new ConcurrentDictionary<string, ProjectAnalyzer>();

        /// <summary>
        /// Retrieves an up-to-date analyzer for a source file + assembly name pair.
        /// </summary>
        /// <param name="sourceFilePath">The full path of a source file.</param>
        /// <param name="assemblyName">The name of the assembly that the source file belongs to.</param>
        /// <returns>A ProjectAnalyzer, or null if cannot be retrieved.</returns>
        public static ProjectAnalyzer GetAnalyzer(string sourceFilePath, string assemblyName)
        {
            var projectFilePath = GetProjectFilePath(sourceFilePath, assemblyName);
            if (projectFilePath == null)
                return null;

            return GetProjectAnalyzer(projectFilePath);
        }

        private static ProjectAnalyzer GetProjectAnalyzer(string projectFilePath)
        {
            var projectAnalyzer = _projectFileToAnalyzerMap.GetOrAdd(projectFilePath, i => CreateAnalyzer(i));
            projectAnalyzer.RefreshConfig();
            return projectAnalyzer;
        }

        private static ProjectAnalyzer CreateAnalyzer(string projectFilePath)
        {
            return new ProjectAnalyzer(projectFilePath);
        }

        private static string GetProjectFilePath(string sourceFilePath, string assemblyName)
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
                    directoryPath = parentDirectory == null ? null : parentDirectory.FullName;
                }

                return null;
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Exception in FindProjectFile({0}, {1}): {2}", sourceFilePath, assemblyName, e));
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
                Trace.WriteLine(string.Format("Exception in IsProjectFileForAssembly({0}, {1}): {2}", projectFilePath, assemblyName, e));
                return false;
            }
        }
    }
}
