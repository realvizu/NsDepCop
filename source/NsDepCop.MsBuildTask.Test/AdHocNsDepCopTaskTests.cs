using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// Testbed for adhoc NsDepCopTask tests to repro bugs found in the context of specific C# projects.
    /// </summary>
    [TestClass]
    public class AdHocNsDepCopTaskTests : NsDepCopTaskTestBase
    {
        private const string ReferenceAssemblyFolderRoot = @"c:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\";
        private const string CsprojXmlNamespace = @"http://schemas.microsoft.com/developer/msbuild/2003";


        [TestMethod]
        [Ignore] // For manual running only
        public void TestACsproj()
        {
            RunNsDepCopTaskForCsproj(@"C:\Work\GitHub\softvis\SoftVis.Diagramming\SoftVis.Diagramming\SoftVis.Diagramming.csproj");
        }

        private static void RunNsDepCopTaskForCsproj(string csprojWithFullPath)
        {
            var baseDirectory = Path.GetDirectoryName(csprojWithFullPath);

            var csprojXml = XDocument.Load(csprojWithFullPath);

            var sourceFileNames = GetSourceFileNames(csprojXml, CsprojXmlNamespace);
            var referencedAssemblyPaths = GetReferencedAssemblyPaths(csprojXml, CsprojXmlNamespace, baseDirectory);

            var nsDepCopTask = CreateNsDepCopTaskWithStubBuildEngine(baseDirectory, sourceFileNames, referencedAssemblyPaths);
            nsDepCopTask.Execute();
        }

        private static IEnumerable<string> GetSourceFileNames(XDocument csprojXml, XNamespace ns)
        {
            return csprojXml.Descendants(ns + "Compile").Attributes("Include").Select(i => i.Value);
        }

        private static IEnumerable<string> GetReferencedAssemblyPaths(XDocument csprojXml, XNamespace ns, string baseDirectory)
        {
            var targetFrameworkVersion = csprojXml.Descendants(ns + "TargetFrameworkVersion").First().Value;
            var assemblyFolder = Path.Combine(Path.Combine(ReferenceAssemblyFolderRoot), targetFrameworkVersion);
            yield return Path.Combine(assemblyFolder, "mscorlib.dll");

            var assemblyReferenceElements = csprojXml.Descendants(ns + "Reference").ToList();
            foreach (var assemblyReferenceElement in assemblyReferenceElements)
            {
                var hintPath = assemblyReferenceElement.Element(ns + "HintPath")?.Value;
                if (hintPath != null)
                {
                    yield return Path.Combine(baseDirectory, hintPath);
                }
                else
                {
                    var assemblyName = assemblyReferenceElement.Attribute("Include").Value;
                    var assemblyFile = Path.Combine(assemblyFolder, assemblyName + ".dll");
                    yield return assemblyFile;
                }
            }
        }

        private static NsDepCopTask CreateNsDepCopTaskWithStubBuildEngine(string baseDirectory, 
            IEnumerable<string> sourceFileNames, IEnumerable<string> referencedFilePaths)
        {
            var mockBuildEngine = MockRepository.GenerateStub<IBuildEngine>();

            var nsDepCopTask = new NsDepCopTask
            {
                BaseDirectory = new TestTaskItem(baseDirectory),
                Compile = CreateTaskItems(CreateFullPathFileNames(baseDirectory, sourceFileNames)),
                ReferencePath = CreateTaskItems(referencedFilePaths),
                BuildEngine = mockBuildEngine,
            };

            return nsDepCopTask;
        }
    }
}
