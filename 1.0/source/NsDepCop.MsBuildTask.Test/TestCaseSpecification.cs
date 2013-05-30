using System.Collections.Generic;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// The parameters that specify a test case.
    /// </summary>
    internal struct TestCaseSpecification
    {
        /// <summary>The name of the test files folder.</summary>
        public string TestFilesFolderName;

        /// <summary>A collection of expected log entry parameters.</summary>
        public IEnumerable<LogEntryParameters> ExpectedLogEntries;

        /// <summary>A collection of name of the source files in the project to be analyzed.</summary>
        public IEnumerable<string> SourceFileNames;

        /// <summary>A collection of the full path of the files referenced in the project.</summary>
        public IEnumerable<string> ReferencedFilePaths;

        /// <summary>
        /// Creates a new instance by setting all fields.
        /// </summary>
        public TestCaseSpecification(
            string testFilesFolderName,
            IEnumerable<LogEntryParameters> expectedLogEntries,
            IEnumerable<string> sourceFileNames = null,
            IEnumerable<string> referencedFilePaths = null
            )
        {
            TestFilesFolderName = testFilesFolderName;
            ExpectedLogEntries = expectedLogEntries;
            SourceFileNames = sourceFileNames;
            ReferencedFilePaths = referencedFilePaths;
        }
    }
}
