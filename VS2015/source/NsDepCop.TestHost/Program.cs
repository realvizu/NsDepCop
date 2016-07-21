﻿using System;
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
            if (args.Length < 1 || args.Length > 2)
            {
                Usage();
                return -1;
            }

            var parserType = GetParserType(args);

            var csProjFileName = args[0];
            var csProjParser = new CsProjParser(csProjFileName);
            var configFileName = Path.Combine(Path.GetDirectoryName(csProjFileName), "config.nsdepcop");

            Console.WriteLine($"Analysing {csProjFileName} with {parserType}...");

            var runTimeSpans = new List<TimeSpan>();
            for (int i = 0; i < 4; i++)
                runTimeSpans.Add(AnalyseCsProj(parserType, configFileName, csProjParser));

            DumpRunTimes(runTimeSpans);

            return 0;
        }

        private static ParserType GetParserType(string [] args)
        {
            var parserType = ParserType.Roslyn;
            if (args.Length == 2)
            {
                if (!Enum.TryParse(args[1], out parserType))
                    Console.WriteLine($"Cannot parse '{args[1]}' ParserType, using {parserType} as default.");
            }
            return parserType;
        }

        private static TimeSpan AnalyseCsProj(ParserType parserType, string configFileName, CsProjParser csProjParser)
        {
            var startTime = DateTime.Now;

            var analyzer = DependencyAnalyzerFactory.Create(parserType, configFileName);
            var dependencyViolations = analyzer.AnalyzeProject(csProjParser.SourceFilePaths, csProjParser.ReferencedAssemblyPaths).ToList();

            var endTime = DateTime.Now;
            var elapsedTimeSpan = endTime - startTime;
            Console.WriteLine($"Analysis took: {elapsedTimeSpan:mm\\:ss\\.fff}");

            DumpDependencyViolations(dependencyViolations);

            return elapsedTimeSpan;
        }

        private static void DumpDependencyViolations(IReadOnlyCollection<DependencyViolation> dependencyViolations)
        {
            Console.WriteLine($"DependencyViolations.Count={dependencyViolations.Count}");
            foreach (var dependencyViolation in dependencyViolations)
                Console.WriteLine(Constants.IllegalDependencyIssue.GetDynamicDescription(dependencyViolation));
        }

        private static void DumpRunTimes(List<TimeSpan> runTimeSpans)
        {
            var averageTimeSpan = TimeSpan.FromMilliseconds(runTimeSpans.Skip(1).Average(i => i.TotalMilliseconds));
            Console.WriteLine($"Average run time (skipping 1st):{averageTimeSpan:mm\\:ss\\.fff}");
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"  {Assembly.GetExecutingAssembly().GetName().Name} <CsProjFileName>");
        }
    }
}
