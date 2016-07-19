using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Codartis.NsDepCop.TestHost
{
    /// <summary>
    /// Parses a C# project file and extracts source file and referenced assembly info.
    /// </summary>
    internal class CsProjParser
    {
        private const string Xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";

        private readonly string _csProjFilePath;
        private readonly string _csProjDirectoryPath;
        private readonly string _mscorlibFilePath;
        private readonly string _frameworkAssemblyPath;
        private List<string> _sourceFilePaths;
        private List<string> _referencedAssemblyPaths;

        public CsProjParser(string csProjFilePath)
        {
            _csProjFilePath = csProjFilePath;
            _csProjDirectoryPath = Path.GetDirectoryName(csProjFilePath);

            _mscorlibFilePath = typeof(int).Assembly.Location;
            _frameworkAssemblyPath = Path.GetDirectoryName(_mscorlibFilePath);

            ParseCsProjFile();
        }

        public IEnumerable<string> SourceFilePaths => _sourceFilePaths;
        public IEnumerable<string> ReferencedAssemblyPaths => _referencedAssemblyPaths;

        private void ParseCsProjFile()
        {
            var document = XDocument.Load(_csProjFilePath);
            if (document.Root == null)
                throw new Exception($"Error loading {_csProjFilePath}");

            _sourceFilePaths = document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("Compile"))
                .Attributes("Include")
                .Select(i => Path.Combine(_csProjDirectoryPath, i.Value))
                .ToList();

            var fileReferences = document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("Reference"))
                .Attributes("Include")
                .Select(i => Path.Combine(_frameworkAssemblyPath, $"{i.Value}.dll"));

            var projectReferences = document.Root
                .Elements(GetXName("ItemGroup"))
                .Elements(GetXName("ProjectReference"))
                .Select(i => Path.Combine(_csProjDirectoryPath, 
                    Path.GetDirectoryName(i.Attribute("Include").Value),
                    "obj\\debug",
                    i.Element(GetXName("Name")).Value + ".dll"));

            _referencedAssemblyPaths = new[] { _mscorlibFilePath }
                .Union(fileReferences)
                .Union(projectReferences)
                .ToList();
        }

        private static XName GetXName(string name)
        {
            return System.Xml.Linq.XName.Get(name, Xmlns);
        }
    }
}