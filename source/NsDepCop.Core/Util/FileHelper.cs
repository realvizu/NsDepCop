using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Finds files in the file system.
    /// </summary>
    internal static class FileHelper
    {
        /// <summary>
        /// Traverses the file system upwards from a given folder and returns the full path whenever a given filename is found.
        /// </summary>
        /// <param name="filenameToFind">The filename to find.</param>
        /// <param name="startingFolderPath">The search starts in this folder (inclusive).</param>
        /// <param name="maxLevel">The maximum number of folder levels to traverse before stopping. 
        /// Must be > 0 or null. Null means unlimited, i.e. stops in the root folder.</param>
        /// <returns>The collection of full paths of the found files.</returns>
        public static IEnumerable<string> FindInParentFolders(string filenameToFind, string startingFolderPath, int? maxLevel = null)
        {
            return GetFilenameWithFolderPaths(filenameToFind, startingFolderPath, maxLevel)
                .Where(File.Exists);
        }

        /// <summary>
        /// Returns a filename combine with a collection of paths, starting from a given folder and traversing upwards the folder tree.
        /// </summary>
        /// <param name="filename">A filename without path.</param>
        /// <param name="startingFolderPath">The full path of the starting folder.</param>
        /// <param name="maxLevel">The maximum number of folder levels to traverse before stopping. 
        /// Must be > 0 or null. Null means unlimited, i.e. stops in the root folder.</param>
        /// <returns>A collection of filenames with full path.</returns>
        public static IEnumerable<string> GetFilenameWithFolderPaths(string filename, string startingFolderPath, int? maxLevel = null)
        {
            return GetFolderPaths(startingFolderPath, maxLevel)
                .Select(i => GetFilePath(i, filename))
                .Where(i => i != null);
        }

        /// <summary>
        /// Returns folder paths by traversing the file system upwards from a given starting folder.
        /// </summary>
        /// <param name="startingFolderPath">The full path of the starting folder.</param>
        /// <param name="maxLevel">The maximum number of folder levels to traverse before stopping. 
        /// Must be > 0 or null. Null means unlimited, i.e. stops in the root folder.</param>
        /// <returns>A collection of folders with full path.</returns>
        public static IEnumerable<string> GetFolderPaths(string startingFolderPath, int? maxLevel = null)
        {
            if (maxLevel.HasValue && maxLevel.Value <= 0)
                throw new ArgumentException("Must be > 0", nameof(maxLevel));

            if (!Directory.Exists(startingFolderPath))
                throw new ArgumentException("Starting folder does not exist or you don't have permission to access it.");

            var level = 0;
            var folderPath = startingFolderPath;

            while (folderPath != null && !IsMaxLevelReached(level, maxLevel))
            {
                yield return folderPath;

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
