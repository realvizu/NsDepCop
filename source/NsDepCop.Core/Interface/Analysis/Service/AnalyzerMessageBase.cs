using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Service
{
    /// <summary>
    /// A message returned by the analyzer service. Immutable.
    /// </summary>
    [Serializable]
    public class AnalyzerMessageBase
    {
        public IllegalDependencyMessage IllegalDependencyMessage { get; }
        public TraceMessage TraceMessage { get; }

        public AnalyzerMessageBase(IllegalDependencyMessage illegalDependencyMessage)
        {
            IllegalDependencyMessage = illegalDependencyMessage;
        }

        public AnalyzerMessageBase(TraceMessage traceMessage)
        {
            TraceMessage = traceMessage;
        }
    }
}
