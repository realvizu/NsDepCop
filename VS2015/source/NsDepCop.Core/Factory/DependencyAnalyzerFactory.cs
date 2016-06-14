using System;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Implementation.Common;
using Codartis.NsDepCop.Core.Interface;
using RoslynImplementation = Codartis.NsDepCop.Core.Implementation.Roslyn;
using NRefactoryImplementation = Codartis.NsDepCop.Core.Implementation.NRefactory;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Factory for IDependencyAnalyzer objects.
    /// </summary>
    public static class DependencyAnalyzerFactory
    {
        /// <summary>
        /// Returns a new IDependencyAnalyzer object using the specified parser name.
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

            return Create(parserType, config);
        }

        /// <summary>
        /// Returns a new IDependencyAnalyzer object using the specified parser type.
        /// </summary>
        /// <param name="parserType">The type of the parser to be used.</param>
        /// <param name="config">The config object required by the analyzer.</param>
        /// <returns>A new IDependencyAnalyzer object using the specified parser.</returns>
        public static IDependencyAnalyzer Create(ParserType parserType, NsDepCopConfig config)
        {
            var typeDependencyValidator = new TypeDependencyValidator(
                config.AllowedDependencies,
                config.DisallowedDependencies,
                config.ChildCanDependOnParentImplicitly,
                config.VisibleTypesByNamespace);

            switch (parserType)
            {
                case ParserType.Roslyn:
                    return new RoslynImplementation.DependencyAnalyzer(config, typeDependencyValidator);

                case ParserType.NRefactory:
                    return new NRefactoryImplementation.DependencyAnalyzer(config, typeDependencyValidator);

                default:
                    throw new Exception($"Unexpected Parser: {parserType}.");
            }
        }
    }
}
