using System;
using System.Collections.Generic;
using System.IO;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Finds files in the file system.
    /// </summary>
    internal static class FileFinder
    {
        /// <summary>
        /// Traverses the file system upwards and returns the full path whenever a given filename is found.
        /// </summary>
        /// <param name="filenameToFind">The filename to find.</param>
        /// <param name="startingFolderPath">The search starts in this folder (inclusive).</param>
        /// <param name="maxLevel">The maximum number of folder levels to search before stopping. 
        /// Must be > 0 or null. Null means unlimited, i.e. search stops in root folder.</param>
        /// <returns>The collection of full paths of the found files.</returns>
        public static IEnumerable<string> FindInParentFolders(string filenameToFind, string startingFolderPath, int? maxLevel = null)
        {
            if (maxLevel.HasValue && maxLevel.Value <= 0)
                throw new ArgumentException("Must be > 0", nameof(maxLevel));

            var level = 0;
            var folderPath = startingFolderPath;

            while (folderPath != null && !IsMaxLevelReached(level, maxLevel))
            {
                var configFilePathCandidate = GetFilePath(folderPath, filenameToFind);

                if (File.Exists(configFilePathCandidate))
                    yield return configFilePathCandidate;

                folderPath = GetParentFolder(folderPath);
                level++;
            }
        }

        private static bool IsMaxLevelReached(int level, int? maxLevel)
        {
            return maxLevel.HasValue
                   && level >= maxLevel.Value;
        }

        private static string GetFilePath(string folderPath, string filename)
        {
            try
            {
                return Path.Combine(folderPath, filename);
            }
            catch
            {
                return null;
            }
        }

        private static string GetParentFolder(string folderPath)
        {
            try
            {
                return Directory.GetParent(folderPath)?.FullName;
            }
            catch
            {
                return null;
            }
        }
    }
}
