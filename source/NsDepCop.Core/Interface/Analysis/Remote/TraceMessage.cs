using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// A trace message.
    /// </summary>
    [Serializable]
    public class TraceMessage : AnalyzerMessageBase
    {
        public string Message { get; }

        public TraceMessage(string message)
        {
            Message = message;
        }
    }
}
