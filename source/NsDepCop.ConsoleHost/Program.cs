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
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

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
            var compilationInfo = GetCompilationInfo(options.CsprojFile);

            var runTimeSpans = new List<TimeSpan>();
            AnalyzerMessageBase[] lastMessages = null;
            for (var i = 0; i < options.RepeatCount; i++)
            {
                if (_isVerbose) Console.WriteLine();
                if (options.RepeatCount > 1)
                    Console.WriteLine($"Starting iteration {i + 1}...");
                if (_isVerbose) Console.WriteLine();

                var (runTime, analyzerMessages) = AnalyseCsProj(analyzer, compilationInfo);

                runTimeSpans.Add(runTime);
                lastMessages = analyzerMessages;
            }

            Console.WriteLine();
            DumpAnalyzerMessages(lastMessages);

            Console.WriteLine();
            for (var i = 0; i < options.RepeatCount; i++)
            {
                var messagePrefix = options.RepeatCount > 1 ? $"Iteration {i + 1:00}" : "Analysis";
                Console.WriteLine($"{messagePrefix} took: {runTimeSpans[i]:mm\\:ss\\.fff}");
            }

            Console.WriteLine();
            DumpRunTimes(runTimeSpans);
        }

        private static CompilationInfo GetCompilationInfo(string csProjFilePath)
        {
            var isNewProjectType = NewCsProjParser.CanParse(csProjFilePath);

            var csProjParser = isNewProjectType
                ? (ICompilationInfoProvider) new NewCsProjParser(csProjFilePath)
                : new LegacyCsProjParser(csProjFilePath);

            return csProjParser.GetCompilationInfo();
        }


        private static IDependencyAnalyzer CreateAnalyzer(string configFolderPath, bool useOutOfProcessAnalyzer)
        {
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(LogTraceToConsole);
            var analyzerFactory = new DependencyAnalyzerFactory(LogTraceToConsole);

            return useOutOfProcessAnalyzer
                ? analyzerFactory.CreateOutOfProcess(configFolderPath, ServiceAddressProvider.ServiceAddress)
                : analyzerFactory.CreateInProcess(configFolderPath, typeDependencyEnumerator);
        }

        private static (TimeSpan runTime, AnalyzerMessageBase[] analyzerMessages) AnalyseCsProj(
            IDependencyAnalyzer dependencyAnalyzer,
            CompilationInfo compilationInfo)
        {
            var startTime = DateTime.Now;

            var analyzerMessages = dependencyAnalyzer
                .AnalyzeProject(compilationInfo.SourceFilePaths, compilationInfo.ReferencedAssemblyPaths)
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
            if (runTimeSpans.Count <= 1)
                return;

            var minRunTimeSpan = TimeSpan.FromMilliseconds(runTimeSpans.Min(i => i.TotalMilliseconds));
            Console.WriteLine($"Min run time: {minRunTimeSpan:mm\\:ss\\.fff}");
        }
    }
}