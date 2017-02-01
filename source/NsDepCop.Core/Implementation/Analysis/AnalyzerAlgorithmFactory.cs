using System;
using Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory;
using Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public static class AnalyzerAlgorithmFactory
    {
        public static IDependencyAnalyzerLogic Create(IProjectConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            switch (config.Parser)
            {
                case Parsers.Roslyn:
                    return new RoslynAnalyzerLogic(config);

                case Parsers.NRefactory:
                    return new NRefactoryAnalyzerLogic(config);

                default:
                    throw new Exception($"Unexpected Parser: {config.Parser}.");
            }
        }
    }
}
