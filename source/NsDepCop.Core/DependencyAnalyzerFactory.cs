using System;
using System.Diagnostics;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Factory for IDependencyAnalyzer objects.
    /// </summary>
    public static class DependencyAnalyzerFactory
    {
        /// <summary>
        /// Returns a new IDependencyAnalyzer object using the specified parser.
        /// </summary>
        /// <param name="parserName">The name of the parser to be used.</param>
        /// <param name="config">The config object required by the analyzer.</param>
        /// <param name="defaultParserType">The parser type to be used as default.</param>
        /// <returns>A new IDependencyAnalyzer object using the specified parser.</returns>
        public static IDependencyAnalyzer Create(string parserName, NsDepCopConfig config, ParserType defaultParserType)
        {
            ParserType parserType;
            if (!Enum.TryParse(parserName, out parserType))
            {
                parserType = defaultParserType;
                Debug.WriteLine($"Unrecognized parser name: '{parserName}'. Using: '{parserType}'.", Constants.TOOL_NAME);
            }

            switch (parserType)
            {
                case ParserType.Roslyn:
                    return new Analyzer.Roslyn.DependencyAnalyzer(config);

                case ParserType.NRefactory:
                    return new Analyzer.NRefactory.DependencyAnalyzer(config);

                default:
                    throw new Exception($"Unexpected Parser: {parserType}.");
            }
        }
    }
}
