using System.Globalization;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    /// <summary>
    /// Defines the diagnostics that the tool can report.
    /// </summary>
    public static class DiagnosticDefinitions
    {
        /// <summary>
        /// Format string to create a help link for diagnostics. The parameter is the ID of the diagnostic in lowercase.
        /// </summary>
        private const string HelpLinkFormat = @"https://github.com/realvizu/NsDepCop/blob/master/doc/Diagnostics.md#{0}";

        public static readonly DiagnosticDescriptor IllegalDependency = CreateDiagnosticDescriptor(
            "NSDEPCOP01",
            "Illegal namespace reference.",
            "Illegal namespace reference: {0}->{1} (Type: {2}->{3}){4}",
            DiagnosticSeverity.Warning,
            "This type cannot reference the other type because their namespaces cannot depend on each other according to the current rules." +
            " Change the dependency rules in the 'config.nsdepcop' file or change your design to avoid this namespace dependency."
        );

        public static readonly DiagnosticDescriptor TooManyDependencyIssues = CreateDiagnosticDescriptor(
            "NSDEPCOP02",
            "Too many dependency issues, analysis was stopped.",
            "Maximum dependency issue count ({0}) reached, analysis in this compilation was stopped.",
            DiagnosticSeverity.Warning,
            "The number of dependency issues in this compilation has exceeded the configured maximum value." +
            " Correct the reported issues and run the build again or set the MaxIssueCount attribute in your 'config.nsdepcop' file to a higher number."
        );

        public static readonly DiagnosticDescriptor NoConfigFile = CreateDiagnosticDescriptor(
            "NSDEPCOP03",
            "No config file found, analysis skipped.",
            "No config file found, analysis skipped.",
            DiagnosticSeverity.Info,
            "This analyzer requires that you add a file called 'config.nsdepcop' to your project with build action 'C# analyzer additional file'." +
            " If there's no such file, the analyzer skips this project."
        );

        public static readonly DiagnosticDescriptor ConfigDisabled = CreateDiagnosticDescriptor(
            "NSDEPCOP04",
            "Analysis is disabled in the config file.",
            "Analysis is disabled in the file 'config.nsdepcop'.",
            DiagnosticSeverity.Info,
            "The IsEnabled attribute was set to false in this project's 'config.nsdepcop' file, so the analyzer skips this project."
        );

        public static readonly DiagnosticDescriptor ConfigException = CreateDiagnosticDescriptor(
            "NSDEPCOP05",
            "Error loading config.",
            "Error when loading the file 'config.nsdepcop': {0}",
            DiagnosticSeverity.Error,
            "There was an error while loading the 'config.nsdepcop' file, see the message for details." +
            " Some common reasons: malformed content, file permission or file locking problem."
        );

        public static readonly DiagnosticDescriptor ToolDisabled = CreateDiagnosticDescriptor(
            "NSDEPCOP06",
            "Analysis is disabled with environment variable.",
            $"NsDepCop is disabled with environment variable: '{ProductConstants.DisableToolEnvironmentVariableName}'.",
            DiagnosticSeverity.Info,
            $"If the '{ProductConstants.DisableToolEnvironmentVariableName}' environment variable is set to 'True' or '1' then all analysis is skipped."
        );

        public static readonly DiagnosticDescriptor IllegalAssemblyDependency = CreateDiagnosticDescriptor(
            "NSDEPCOP07",
            "Illegal assembly reference.",
            "Illegal assembly reference: {0}->{1}",
            DiagnosticSeverity.Warning,
            "This assembly cannot reference the other assembly because their dependency is prohibit according to the current rules." +
            " Change the dependency rules in the 'config.nsdepcop' file or change your design to avoid this assembly dependency."
        );

        private static DiagnosticDescriptor CreateDiagnosticDescriptor(
            string id,
            string title,
            string messageFormat,
            DiagnosticSeverity defaultSeverity,
            string description)
        {
            return new(
                id,
                title,
                messageFormat,
                category: ProductConstants.ToolName,
                defaultSeverity,
                isEnabledByDefault: true,
                description,
                helpLinkUri: string.Format(HelpLinkFormat, id.ToLower(CultureInfo.InvariantCulture))
            );
        }
    }
}