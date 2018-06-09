using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that an analysis has started.
    /// </summary>
    [Serializable]
    public class AnalysisStartedMessage : InfoMessageBase
    {
        public string ProjectLocation { get; }

        public AnalysisStartedMessage(string projectLocation)
            : base(InfoMessageType.AnalysisStarted)
        {
            ProjectLocation = projectLocation;
        }

        public override string ToString() => $"Analysing project in folder: {ProjectLocation}";
    }
}