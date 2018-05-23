using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// Abstract base class for informational messages.
    /// </summary>
    [Serializable]
    public abstract class InfoMessageBase : AnalyzerMessageBase
    {
        public InfoMessageType Type { get; }

        protected InfoMessageBase(InfoMessageType type)
        {
            Type = type;
        }
    }
}
