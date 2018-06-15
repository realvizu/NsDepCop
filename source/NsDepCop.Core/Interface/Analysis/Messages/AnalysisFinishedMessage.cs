using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that an analysis has finished.
    /// </summary>
    public sealed class AnalysisFinishedMessage : InfoMessageBase
    {
        public TimeSpan AnalysisDuration { get; }
        public int DependencyIssueCount { get; }

        public AnalysisFinishedMessage(TimeSpan analysisDuration, int dependencyIssueCount)
        {
            AnalysisDuration = analysisDuration;
            DependencyIssueCount = dependencyIssueCount;
        }

        public override string ToString() => $"Analysis took: {AnalysisDuration:mm\\:ss\\.fff}, dependency issues: {DependencyIssueCount}.";
    }
}