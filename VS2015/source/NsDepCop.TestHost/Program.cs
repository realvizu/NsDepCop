using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.TestHost
{
    /// <summary>
    /// Main for test host.
    /// </summary>
    internal class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Usage();
                return -1;
            }

            var csProjFileName = args[0];
            var csProjParser = new CsProjParser(csProjFileName);
            var configFileName = Path.Combine(Path.GetDirectoryName(csProjFileName), "config.nsdepcop");

            var analyzer = DependencyAnalyzerFactory.Create(ParserType.Roslyn, configFileName);
            var dependencyViolations = analyzer.AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths).ToList();
            DumpDependencyViolations(dependencyViolations);

            return 0;
        }

        private static void DumpDependencyViolations(IReadOnlyCollection<DependencyViolation> dependencyViolations)
        {
            Console.WriteLine($"DependencyViolations.Count={dependencyViolations.Count}");
            foreach (var dependencyViolation in dependencyViolations)
                Console.WriteLine(Constants.IllegalDependencyIssue.GetDynamicDescription(dependencyViolation));
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {Assembly.GetExecutingAssembly().GetName().Name} <CsProjFileName>");
        }
    }
}
