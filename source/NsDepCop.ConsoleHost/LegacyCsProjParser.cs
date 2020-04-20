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
    internal class LegacyCsProjParser : ICompilationInfoProvider
    {
        private static readonly string MscorlibFilePath = typeof(int).Assembly.Location;

        private static readonly List<string> ReferenceDirectories = new List<string>
        {
            @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2",
            @"C:\Program Files (x86)\Microsoft SDKs\Expression\Blend\.NETFramework\v4.5\Libraries"
        };

        private const string Xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";

        private readonly string _csProjFilePath;
        private readonly string _csProjDirectoryPath;

        public LegacyCsProjParser(string csProjFilePath)
        {
            _csProjFilePath = csProjFilePath;
            _csProjDirectoryPath = Path.GetDirectoryName(csProjFilePath);
        }

        public CompilationInfo GetCompilationInfo()
        {
            var document = XDocument.Load(_csProjFilePath);
            if (document.Root == null)
                throw new Exception($"Error loading {_csProjFilePath}");

            var sourceFilePaths = ExtractSourceFilePaths(document);

            var referencedAssemblyPaths = new[] {MscorlibFilePath}
                .Union(GetFileReferencesWithoutHintPath(document))
                .Union(GetFileReferencesWithHintPath(document))
                .Union(ExtractProjectReferences(document))
                .ToList();

            return new CompilationInfo(sourceFilePaths, referencedAssemblyPaths);
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
                .Where(i => !i.Elements(GetXName("HintPath")).Any())
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