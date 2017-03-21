using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using CommandLine;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Implements the console host.
    /// </summary>
    internal class Program
    {
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
            Console.WriteLine($"Analysing {options.CsprojFile}, repeats={options.RepeatCount}, useSingleFileConfig={options.UseSingleFileConfig} ...");

            _isVerbose = options.IsVerbose;

            var csProjParser = new CsProjParser(options.CsprojFile);
            var dependencyAnalyzer = CreateDependencyAnalyzer(options);

            var runTimeSpans = new List<TimeSpan>();
            for (var i = 0; i < options.RepeatCount; i++)
                runTimeSpans.Add(AnalyseCsProj(dependencyAnalyzer, csProjParser));

            DumpRunTimes(runTimeSpans);
        }

        private static IDependencyAnalyzer CreateDependencyAnalyzer(CommandLineOptions options)
        {
            var directoryPath = Path.GetDirectoryName(options.CsprojFile);

            var dependencyAnalyzerFactory = new DependencyAnalyzerFactory(LogInfoToConsole, LogDiagnosticToConsole);

            return options.UseSingleFileConfig
                ? dependencyAnalyzerFactory.CreateFromXmlConfigFile(Path.Combine(directoryPath, "config.nsdepcop"))
                : dependencyAnalyzerFactory.CreateFromMultiLevelXmlConfigFile(directoryPath);
        }

        private static TimeSpan AnalyseCsProj(IDependencyAnalyzer dependencyAnalyzer, CsProjParser csProjParser)
        {
            var startTime = DateTime.Now;

            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths).ToList();

            var endTime = DateTime.Now;
            var elapsedTimeSpan = endTime - startTime;
            Console.WriteLine($"Analysis took: {elapsedTimeSpan:mm\\:ss\\.fff}");

            DumpIllegalDependencies(illegalDependencies);

            return elapsedTimeSpan;
        }

        private static void LogInfoToConsole(string message)
        {
            Console.WriteLine(message);
        }

        private static void LogDiagnosticToConsole(string message)
        {
            if (_isVerbose) Console.WriteLine(message);
        }

        private static void DumpIllegalDependencies(IReadOnlyCollection<TypeDependency> typeDependencies)
        {
            Console.WriteLine($"Illegal dependencies count={typeDependencies.Count}");
            foreach (var typeDependency in typeDependencies)
                Console.WriteLine(FormatIssue(typeDependency));
        }

        private static string FormatIssue(TypeDependency typeDependency)
        {
            return $"{IssueDefinitions.IllegalDependencyIssue.GetDynamicDescription(typeDependency)} at {typeDependency.SourceSegment}";
        }

        private static void DumpRunTimes(List<TimeSpan> runTimeSpans)
        {
            var minRunTimeSpan = TimeSpan.FromMilliseconds(runTimeSpans.Min(i => i.TotalMilliseconds));
            Console.WriteLine($"Min run time: {minRunTimeSpan:mm\\:ss\\.fff}");
        }
    }
}
