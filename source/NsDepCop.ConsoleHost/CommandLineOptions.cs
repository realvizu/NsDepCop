using CommandLine;
using CommandLine.Text;

namespace Codartis.NsDepCop.ConsoleHost
{
    /// <summary>
    /// Describes the command line options.
    /// </summary>
    internal class CommandLineOptions
    {
        [Option('f', "projectfile", Required = true, HelpText = "The name and path of the csproj file to validate.")]
        public string CsprojFile { get; set; }

        [Option('r', "repeats", DefaultValue = 3, HelpText = "Repeats the validation the given times. Used for average run time measurement.")]
        public int RepeatCount { get; set; }

        [Option('v', "verbose", DefaultValue = false, HelpText = "Verbose output with internal diagnostic messages.")]
        public bool IsVerbose { get; set; }

        [Option('s', "singleconfig", DefaultValue = false, HelpText = "Uses a single config file (not multi level).")]
        public bool UseSingleFileConfig { get; set; }

        [Option('o', "outofprocess", DefaultValue = false, HelpText = "Runs analysis in separate service process (NsDepCop.ServiceHost).")]
        public bool UseOufOfProcessAnalyzer { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, i => HelpText.DefaultParsingErrorsHandler(this, i));
        }
    }
}
