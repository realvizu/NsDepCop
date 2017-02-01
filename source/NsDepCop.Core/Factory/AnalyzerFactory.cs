using System;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory;
using Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public class AnalyzerFactory : IAnalyzerFactory
    {
        public IDependencyAnalyzer CreateDependencyAnalyzer(IProjectConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            switch (config.Parser)
            {
                case Parsers.Roslyn:
                    return new RoslynDependencyAnalyzer(config);

                case Parsers.NRefactory:
                    return new NRefactoryDependencyAnalyzer(config);

                default:
                    throw new Exception($"Unexpected Parser: {config.Parser}.");
            }
        }
    }
}
