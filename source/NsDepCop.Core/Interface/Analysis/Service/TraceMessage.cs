using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Service
{
    /// <summary>
    /// A trace message. Can contain multiple strings.
    /// </summary>
    [Serializable]
    public class TraceMessage : AnalyzerMessageBase
    {
        public IEnumerable<string> Messages { get; }

        public TraceMessage(IEnumerable<string> messages)
        {
            Messages = messages;
        }
    }
}
