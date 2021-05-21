namespace Codartis.NsDepCop.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that an analysis has started.
    /// </summary>
    public sealed class AnalysisStartedMessage : InfoMessageBase
    {
        public string ProjectLocation { get; }

        public AnalysisStartedMessage(string projectLocation)
        {
            ProjectLocation = projectLocation;
        }

        public override string ToString() => $"Analysing project in folder: {ProjectLocation}";
    }
}