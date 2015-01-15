using Codartis.NsDepCop.Core.Common;
using System;
using System.Diagnostics;

namespace Codartis.NsDepCop.Core.Analyzer.Factory
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
        /// <returns>A new IDependencyAnalyzer object using the specified parser.</returns>
        public static IDependencyAnalyzer Create(string parserName, NsDepCopConfig config)
        {
            Parser parser;
            if (!Enum.TryParse(parserName, out parser))
            {
                parser = Parser.Roslyn;
                Debug.WriteLine("Unrecognized parser: '{0}'. Using: '{1}'.", parserName, parser);
            }

            switch (parser)
            {
                case (Parser.Roslyn):
                    return new Codartis.NsDepCop.Core.Analyzer.Roslyn.DependencyAnalyzer(config);

                case (Parser.NRefactory):
                    return new Codartis.NsDepCop.Core.Analyzer.NRefactory.DependencyAnalyzer(config);

                default:
                    throw new Exception(string.Format("Unexpected Parser: {0}.", parser));
            }
        }
    }
}
