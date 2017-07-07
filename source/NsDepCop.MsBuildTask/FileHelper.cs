using System.IO;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class FileHelper
    {
        /// <summary>
        /// Makes a path absolute. If it was already absolute then returns it unchanged.
        /// </summary>
        /// <param name="absoluteOrRelativePath">The path to make absolute.</param>
        /// <param name="relativeTo">The path prefix that makes relative paths absolute.</param>
        /// <returns>An absolute path.</returns>
        public static string ToAbsolutePath(this string absoluteOrRelativePath, string relativeTo)
            => Path.IsPathRooted(absoluteOrRelativePath)
                ? absoluteOrRelativePath
                : Path.Combine(relativeTo, absoluteOrRelativePath);
    }
}