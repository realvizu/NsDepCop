using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Parses a C# project file and extracts source file and referenced assembly info.
    /// </summary>
    internal class CsProjParser
    {
        private static readonly List<string> ReferenceDirectories = new List<string>
        {
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2",
            @"C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
        };

        private const string Xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";

        private readonly string _csProjFilePath;
        private readonly string _csProjDirectoryPath;
        private readonly string _mscorlibFilePath;
        private List<string> _sourceFilePaths;
        private List<string> _referencedAssemblyPaths;

        public CsProjParser(string csProjFilePath)
        {
            _csProjFilePath = csProjFilePath;
            _csProjDirectoryPath = Path.GetDirectoryName(csProjFilePath);

            _mscorlibFilePath = typeof(int).Assembly.Location;

            ParseCsProjFile();
        }

        public IEnumerable<string> SourceFilePaths => _sourceFilePaths;
        public IEnumerable<string> ReferencedAssemblyPaths => _referencedAssemblyPaths;

        private void ParseCsProjFile()
        {
            var document = XDocument.Load(_csProjFilePath);
            if (document.Root == null)
                throw new Exception($"Error loading {_csProjFilePath}");

            _sourceFilePaths = ExtractSourceFilePaths(document);

            _referencedAssemblyPaths = new[] { _mscorlibFilePath }
                .Union(GetFileReferencesWithoutHintPath(document))
                .Union(GetFileReferencesWithHintPath(document))
                .Union(ExtractProjectReferences(document))
                .ToList();
        }

        private List<string> ExtractSourceFilePaths(XDocument document)
        {
            return document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("Compile"))
                .Attributes("Include")
                .Select(i => Path.Combine(_csProjDirectoryPath, i.Value))
                .ToList();
        }

        private static IEnumerable<string> GetFileReferencesWithoutHintPath(XDocument document)
        {
            return document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("Reference"))
                .Where(i=>!i.Elements(GetXName("HintPath")).Any())
                .Attributes("Include")
                .Select(i => $"{GetAssemblyFileName(i.Value)}.dll")
                .SelectMany(CombineWithPossiblePaths)
                .Where(File.Exists);
        }

        private static IEnumerable<string> CombineWithPossiblePaths(string s)
        {
            return ReferenceDirectories.Select(i => Path.Combine(i, s));
        }

        private static string GetAssemblyFileName(string i)
        {
            var spaceIndex = i.IndexOf(',');
            return spaceIndex >= 0 
                ? i.Substring(0, spaceIndex) 
                : i;
        }

        private IEnumerable<string> GetFileReferencesWithHintPath(XDocument document)
        {
            return document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("Reference"))
                .Elements(GetXName("HintPath"))
                .Select(i => Path.Combine(_csProjDirectoryPath, $"{i.Value}"));
        }

        private IEnumerable<string> ExtractProjectReferences(XDocument document)
        {
            return document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("ProjectReference"))
                .Select(i => Path.Combine(_csProjDirectoryPath, 
                    Path.GetDirectoryName(i.Attribute("Include").Value),
                    "obj\\debug",
                    i.Element(GetXName("Name")).Value + ".dll"));
        }

        private static XName GetXName(string name) => XName.Get(name, Xmlns);
    }
}