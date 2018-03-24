using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    /// <summary>
    /// Builds the result array of the AnalyzeProject operation.
    /// </summary>
    internal class AnalyzeProjectResultBuilder
    {
        private readonly List<AnalyzerMessageBase> _buffer = new List<AnalyzerMessageBase>();

        public void AddTrace(string message) => _buffer.Add(new TraceMessage(message));
        public void AddIllegalDependency(TypeDependency typeDependency) => _buffer.Add(new IllegalDependencyMessage(typeDependency));
        public void AddCacheStatistics(int hits, int misses, double efficiency) => _buffer.Add(new CacheStatisticsMessage(hits, misses, efficiency));

        public AnalyzerMessageBase[] ToArray() => _buffer.ToArray();
    }
}
