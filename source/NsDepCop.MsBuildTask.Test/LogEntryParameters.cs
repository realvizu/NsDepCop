using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// The parameters of an MsBuildTask log entry that a unit test can check.
    /// </summary>
    internal struct LogEntryParameters
    {
        public string Code;
        public IssueKind IssueKind;
        public string Path;
        public int StartLine; 
        public int StartColumn;
        public int EndLine;
        public int EndColumn;

        public static LogEntryParameters FromIssueDescriptor(IssueDescriptor issueDescriptor)
        {
            return new LogEntryParameters
            {
                Code = issueDescriptor.Id,
                IssueKind = issueDescriptor.DefaultKind
            };
        }
    }
}
