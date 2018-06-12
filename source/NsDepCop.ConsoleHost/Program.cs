using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
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
            Console.WriteLine($"  outofprocess={options.UseOufOfProcessAnalyzer}");
            Console.WriteLine($"  verbose={options.IsVerbose}");
            Console.WriteLine();

            _isVerbose = options.IsVerbose;

            var directoryPath = Path.GetDirectoryName(options.CsprojFile);
            if (directoryPath == null)
                throw new Exception("DirectoryPath is null.");

            var analyzer = CreateAnalyzer(directoryPath, options.UseOufOfProcessAnalyzer);
            var csProjParser = new CsProjParser(options.CsprojFile);

            var runTimeSpans = new List<TimeSpan>();
            AnalyzerMessageBase[] lastMessages = null;
            for (var i = 0; i < options.RepeatCount; i++)
            {
                if (_isVerbose) Console.WriteLine();
                Console.WriteLine($"Starting iteration {i + 1}...");
                if (_isVerbose) Console.WriteLine();

                var (runTime, analyzerMessages) = AnalyseCsProj(analyzer, csProjParser);

                runTimeSpans.Add(runTime);
                lastMessages = analyzerMessages;
            }

            Console.WriteLine();
            DumpAnalyzerMessages(lastMessages);

            Console.WriteLine();
            for (var i = 0; i < options.RepeatCount; i++)
                Console.WriteLine($"Iteration {i + 1:00} took: {runTimeSpans[i]:mm\\:ss\\.fff}");

            Console.WriteLine();
            DumpRunTimes(runTimeSpans);
        }

        private static IDependencyAnalyzer CreateAnalyzer(string configFolderPath, bool useOutOfProcessAnalyzer)
        {
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(LogTraceToConsole);
            var analyzerFactory = new DependencyAnalyzerFactory(LogTraceToConsole);

            return useOutOfProcessAnalyzer
                ? analyzerFactory.CreateOutOfProcess(configFolderPath, ServiceAddressProvider.ServiceAddress)
                : analyzerFactory.CreateInProcess(configFolderPath, typeDependencyEnumerator);
        }

        private static (TimeSpan runTime, AnalyzerMessageBase[] analyzerMessages) 
            AnalyseCsProj(IDependencyAnalyzer dependencyAnalyzer, CsProjParser csProjParser)
        {
            var startTime = DateTime.Now;

            var analyzerMessages = dependencyAnalyzer
                .AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths)
                .ToArray();

            var endTime = DateTime.Now;
            var elapsedTimeSpan = endTime - startTime;

            return (elapsedTimeSpan, analyzerMessages);
        }

        private static void LogTraceToConsole(string message)
        {
            if (_isVerbose)
                Console.WriteLine(message);
        }

        private static void DumpAnalyzerMessages(AnalyzerMessageBase[] analyzerMessages)
        {
            foreach (var analyzerMessage in analyzerMessages)
                Console.WriteLine(analyzerMessage);
        }

        private static void DumpRunTimes(List<TimeSpan> runTimeSpans)
        {
            var minRunTimeSpan = TimeSpan.FromMilliseconds(runTimeSpans.Min(i => i.TotalMilliseconds));
            Console.WriteLine($"Min run time: {minRunTimeSpan:mm\\:ss\\.fff}");
        }
    }
}
