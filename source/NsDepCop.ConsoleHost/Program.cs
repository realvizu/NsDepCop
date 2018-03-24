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
    public static class Program
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
            Console.WriteLine($"Analysing {options.CsprojFile}");
            Console.WriteLine($"  repeats={options.RepeatCount}");
            Console.WriteLine($"  useSingleFileConfig={options.UseSingleFileConfig}");
            Console.WriteLine($"  outofprocess={options.UseOufOfProcessAnalyzer}");
            Console.WriteLine($"  verbose={options.IsVerbose}");
            Console.WriteLine();

            _isVerbose = options.IsVerbose;

            var directoryPath = Path.GetDirectoryName(options.CsprojFile);
            if (directoryPath == null)
                throw new Exception("DirectoryPath is null.");

            var configProvider = CreateConfigProvider(directoryPath, options.UseSingleFileConfig);
            var analyzer = CreateAnalyzer(configProvider.Config, options.UseOufOfProcessAnalyzer);

            var csProjParser = new CsProjParser(options.CsprojFile);

            var runTimeSpans = new List<TimeSpan>();
            TypeDependency[] lastIllegalDependencies = null;
            for (var i = 0; i < options.RepeatCount; i++)
            {
                if (options.IsVerbose) Console.WriteLine();
                Console.WriteLine($"Starting iteration {i + 1}...");
                if (options.IsVerbose) Console.WriteLine();

                var (runTime, illegalDependencies) = AnalyseCsProj(analyzer, csProjParser);

                runTimeSpans.Add(runTime);
                lastIllegalDependencies = illegalDependencies;
            }

            Console.WriteLine();
            DumpIllegalDependencies(lastIllegalDependencies);

            Console.WriteLine();
            for (var i = 0; i < options.RepeatCount; i++)
                Console.WriteLine($"Iteration {i + 1:00} took: {runTimeSpans[i]:mm\\:ss\\.fff}");

            Console.WriteLine();
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

        private static (TimeSpan runTime, TypeDependency[] illegalDependencies) AnalyseCsProj(IDependencyAnalyzer dependencyAnalyzer, CsProjParser csProjParser)
        {
            var startTime = DateTime.Now;

            var illegalDependencies = dependencyAnalyzer
                .AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths)
                .ToArray();

            var endTime = DateTime.Now;
            var elapsedTimeSpan = endTime - startTime;

            return (elapsedTimeSpan, illegalDependencies);
        }

        private static void LogTraceToConsole(string message)
        {
            if (_isVerbose)
                Console.WriteLine(message);
        }

        private static void DumpIllegalDependencies(TypeDependency[] typeDependencies)
        {
            foreach (var typeDependency in typeDependencies)
                Console.WriteLine(FormatIssue(typeDependency));

            Console.WriteLine($"Illegal dependencies count={typeDependencies.Length}");
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
