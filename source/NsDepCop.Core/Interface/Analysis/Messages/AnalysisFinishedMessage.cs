using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that an analysis has finished.
    /// </summary>
    [Serializable]
    public class AnalysisFinishedMessage : InfoMessageBase
    {
        public TimeSpan AnalysisDuration { get; }

        public AnalysisFinishedMessage(TimeSpan analysisDuration)
            : base(InfoMessageType.AnalysisFinished)
        {
            AnalysisDuration = analysisDuration;
        }

        public override string ToString() => $"Analysis took: {AnalysisDuration:mm\\:ss\\.fff}";
    }
}