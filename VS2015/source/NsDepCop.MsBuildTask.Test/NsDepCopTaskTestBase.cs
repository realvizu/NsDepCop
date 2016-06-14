using System.Collections.Generic;
using System.IO;
using System.Linq;
using Codartis.NsDepCop.Core;
using Microsoft.Build.Framework;

namespace Codartis.NsDepCop.MsBuildTask.Test
{
    /// <summary>
    /// This abstract base class defines some utility functions for testing the NsDepCopTask class.
    /// </summary>
    public abstract class NsDepCopTaskTestBase
    {
        /// <summary>
        /// Creates MSBuild ITaskItem array from the given string collection by wrapping the string content into TestTaskItem objects.
        /// </summary>
        /// <param name="payloadCollection">A string collection. The payload of the task items.</param>
        /// <returns>A TaskItem array created from the given payload.</returns>
        protected static ITaskItem[] CreateTaskItems(IEnumerable<string> payloadCollection)
        {
            return payloadCollection.EmptyIfNull().Select(i => new TestTaskItem(i)).OfType<ITaskItem>().ToArray();
        }

        /// <summary>
        /// Creates a collection of full path filenames from the given filenames and a base directory.
        /// </summary>
        /// <param name="baseDirecytory">A directory.</param>
        /// <param name="sourceFileNames">A collection of filenames (without full path).</param>
        /// <returns>A collection of full path filenames.</returns>
        protected static IEnumerable<string> CreateFullPathFileNames(string baseDirecytory, IEnumerable<string> sourceFileNames)
        {
            return sourceFileNames.EmptyIfNull().Select(i => FileNameToFullPath(baseDirecytory, i));
        }

        /// <summary>
        /// Converts a file name to full path by prepending a base directory.
        /// </summary>
        /// <param name="baseDirecytory">The full path of the base directory.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The filename converted to full path.</returns>
        protected static string FileNameToFullPath(string baseDirecytory, string fileName)
        {
            return baseDirecytory != null && fileName != null
                ? Path.Combine(baseDirecytory, fileName)
                : null;
        }
    }
}