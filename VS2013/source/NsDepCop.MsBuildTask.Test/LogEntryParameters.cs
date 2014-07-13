﻿using Codartis.NsDepCop.Core.Common;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// The parameters of an MsBuildTask log entry that a unit test can check.
    /// </summary>
    internal struct LogEntryParameters
    {
        public IssueKind IssueKind;
        public string Code;
        public string Path;
        public int StartLine; 
        public int StartColumn;
        public int EndLine;
        public int EndColumn;
    }
}
