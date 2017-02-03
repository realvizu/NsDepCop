using System;
using Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory;
using Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Creates type dependency enumerator objects.
    /// </summary>
    public static class TypeDependencyEnumeratorFactory
    {
        public static ITypeDependencyEnumerator Create(Parsers parser)
        {
            switch (parser)
            {
                case Parsers.Roslyn:
                    return new RoslynTypeDependencyEnumerator();

                case Parsers.NRefactory:
                    return new NRefactoryTypeDependencyEnumerator();

                default:
                    throw new Exception($"Unexpected Parser: {parser}.");
            }
        }
    }
}
