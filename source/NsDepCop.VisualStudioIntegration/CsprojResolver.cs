using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds the C# project file (csproj) that a source file belongs to.
    /// </summary>
    internal class CsprojResolver : ICsprojResolver
    {
        /// <summary>
        /// Callback for emitting info messages;
        /// </summary>
        private readonly MessageHandler _infoMessageHandler;

        /// <summary>
        /// Callback for emitting diagnostic messages;
        /// </summary>
        private readonly MessageHandler _diagnosticMessageHandler;

        public CsprojResolver(MessageHandler infoMessageHandler = null, MessageHandler diagnosticMessageHandler = null)
        {
            _infoMessageHandler = infoMessageHandler;
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public string GetCsprojFile(string sourceFilePath, string assemblyName)
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
                        {
                            _diagnosticMessageHandler?.Invoke($"Project file for '{sourceFilePath}' is '{projectFilePath}'.");
                            return projectFilePath;
                        }
                    }

                    var parentDirectory = Directory.GetParent(directoryPath);
                    directoryPath = parentDirectory?.FullName;
                }

                return null;
            }
            catch (Exception e)
            {
                _infoMessageHandler?.Invoke($"Exception in FindProjectFile({sourceFilePath}, {assemblyName}): {e}");
                return null;
            }
        }

        private bool IsProjectFileForAssembly(string projectFilePath, string assemblyName)
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
                _infoMessageHandler?.Invoke($"Exception in IsProjectFileForAssembly({projectFilePath}, {assemblyName}): {e}");
                return false;
            }
        }
    }
}
