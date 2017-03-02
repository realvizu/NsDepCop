using System;
using Codartis.NsDepCop.Core.Interface.Config;
using CommandLine;
using CommandLine.Text;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Describes the command line options.
    /// </summary>
    internal class CommandLineOptions
    {
        [Option('f', "projectFile", Required = true, HelpText = "The name and path of the csproj file to validate.")]
        public string CsprojFile { get; set; }

        [Option('p', "parser", HelpText = "Specifies the parser to be used, overrides the parser defined in config.")]
        public Parsers? Parser { get; set; }

        [Option('r', "repeats", DefaultValue = 3, HelpText = "Repeats the validation the given times. Used for average run time measurement.")]
        public int RepeatCount { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Verbose output with internal diagnostic messages.")]
        public bool IsVerbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var helpText = HelpText.AutoBuild(this, i => HelpText.DefaultParsingErrorsHandler(this, i));
            helpText.AddPostOptionsLine(GetEnumValuesHelpText<Parsers>("  Valid parser types are: "));
            return helpText;
        }

        private static string GetEnumValuesHelpText<TEnum>(string prefix)
            where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
                throw new ArgumentException($"{typeof(TEnum).Name} must be an enum.");

            return prefix + string.Join(", ", Enum.GetNames(typeof(TEnum))) + "\n";
        }
    }
}
