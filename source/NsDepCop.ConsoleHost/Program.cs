using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;
using CommandLine;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Implements the console host.
    /// </summary>
    public class Program
    {
        private static bool _isVerbose;

        public static int Main(string[] args)
        {
            var options = new CommandLineOptions();
            if (!Parser.Default.ParseArguments(args, options))
                return -1;

            try
            {
                Execute(options);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return -1;
        }

        private static void Execute(CommandLineOptions options)
        {
            Console.WriteLine($"Analysing {options.CsprojFile}, repeats={options.RepeatCount}, useSingleFileConfig={options.UseSingleFileConfig} ...");

            _isVerbose = options.IsVerbose;

            var directoryPath = Path.GetDirectoryName(options.CsprojFile);
            if (directoryPath == null)
                throw new Exception("DirectoryPath is null.");

            var configProvider = CreateConfigProvider(directoryPath, options.UseSingleFileConfig);
            var analyzer = CreateAnalyzer(configProvider.Config, options.UseRemoteAnalyzer);

            var csProjParser = new CsProjParser(options.CsprojFile);

            var runTimeSpans = new List<TimeSpan>();
            for (var i = 0; i < options.RepeatCount; i++)
                runTimeSpans.Add(AnalyseCsProj(analyzer, csProjParser));

            DumpRunTimes(runTimeSpans);
        }

        private static IConfigProvider CreateConfigProvider(string directoryPath, bool useSingleFileConfig)
        {
            var configProviderFactory = new ConfigProviderFactory(LogTraceToConsole);

            return useSingleFileConfig
                ? configProviderFactory.CreateFromXmlConfigFile(Path.Combine(directoryPath, "config.nsdepcop"))
                : configProviderFactory.CreateFromMultiLevelXmlConfigFile(directoryPath);
        }

        private static IDependencyAnalyzer CreateAnalyzer(IAnalyzerConfig config, bool useRemoteAnalyzer)
        {
            var analyzerFactory = new DependencyAnalyzerFactory(config, LogTraceToConsole);

            if (useRemoteAnalyzer)
                return analyzerFactory.CreateRemote(ServiceAddressProvider.ServiceAddress);

            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(LogTraceToConsole);
            return analyzerFactory.CreateInProcess(typeDependencyEnumerator);
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

        private static void LogTraceToConsole(string message)
        {
            if (_isVerbose)
                Console.WriteLine(message);
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
