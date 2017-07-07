using System;
using System.IO;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Returns the directory of an assembly.
        /// </summary>
        /// <param name="assembly">An assembly.</param>
        /// <returns>The directory of the assembly.</returns>
        public static string GetDirectory(this Assembly assembly)
        {
            var codebase = new Uri(assembly.CodeBase);
            return Path.GetDirectoryName(codebase.AbsolutePath);
        }
    }
}