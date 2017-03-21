using System.Collections.Generic;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// The parameters that specify a test case.
    /// </summary>
    public class TestCaseSpecification
    {
        /// <summary>The name of the test files folder.</summary>
        public string TestFilesFolderName;

        /// <summary>A collection of expected log entry parameters.</summary>
        public IEnumerable<LogEntryParameters> ExpectedLogEntries = null;

        /// <summary>A collection of name of the source files in the project to be analyzed.</summary>
        public IEnumerable<string> SourceFileNames = null;

        /// <summary>A collection of the full path of the files referenced in the project.</summary>
        public IEnumerable<string> ReferencedFilePaths = null;

        /// <summary>If true then sets an expectation that the tool logs an event when it starts.</summary>
        public bool ExpectStartEvent = true;

        /// <summary>If true then sets an expectation that the tool logs an event when it finishes.</summary>
        public bool ExpectEndEvent = true;

        /// <summary>The expected return value of the task.</summary>
        public bool ExpectedReturnValue = true;
    }
}
