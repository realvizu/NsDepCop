using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using CommandLine;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Implements the console host.
    /// </summary>
    internal class Program
    {
        private static readonly DependencyAnalyzerFactory DependencyAnalyzerFactory = new DependencyAnalyzerFactory();
        private static bool _isVerbose;

        public static int Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (!Parser.Default.ParseArguments(args, options))
                return -1;

            ValidateProject(options);
            return 0;
        }

        private static void ValidateProject(CommandLineOptions options)
        {
            _isVerbose = options.IsVerbose;

            var csProjParser = new CsProjParser(options.CsprojFile);
            var configFileName = Path.Combine(Path.GetDirectoryName(options.CsprojFile), "config.nsdepcop");

            Console.WriteLine($"Analysing {options.CsprojFile}, parser={options.Parser?.ToString() ?? "(not specified)"}, repeats={options.RepeatCount} ...");

            var runTimeSpans = new List<TimeSpan>();
            for (var i = 0; i < options.RepeatCount; i++)
                runTimeSpans.Add(AnalyseCsProj(configFileName, csProjParser, options.Parser));

            DumpRunTimes(runTimeSpans);
        }

        private static TimeSpan AnalyseCsProj(string configFileName, CsProjParser csProjParser, Parsers? overridingParser = null)
        {
            var startTime = DateTime.Now;

            var dependencyAnalyzer = DependencyAnalyzerFactory.CreateFromXmlConfigFile(configFileName, overridingParser, LogDiagnosticMessage);
            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths).ToList();

            var endTime = DateTime.Now;
            var elapsedTimeSpan = endTime - startTime;
            Console.WriteLine($"Analysis took: {elapsedTimeSpan:mm\\:ss\\.fff}");

            DumpIllegalDependencies(illegalDependencies);

            return elapsedTimeSpan;
        }

        private static void LogDiagnosticMessage(string message)
        {
            if (_isVerbose) Console.WriteLine(message);
        }

        private static void DumpIllegalDependencies(IReadOnlyCollection<TypeDependency> typeDependencies)
        {
            Console.WriteLine($"Illegal dependencies count={typeDependencies.Count}");
            foreach (var typeDependency in typeDependencies)
                Console.WriteLine(IssueDefinitions.IllegalDependencyIssue.GetDynamicDescription(typeDependency));
        }

        private static void DumpRunTimes(List<TimeSpan> runTimeSpans)
        {
            var minRunTimeSpan = TimeSpan.FromMilliseconds(runTimeSpans.Min(i => i.TotalMilliseconds));
            Console.WriteLine($"Min run time: {minRunTimeSpan:mm\\:ss\\.fff}");
        }
    }
}
