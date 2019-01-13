using System;
using System.IO;
using System.Reflection;

namespace Codartis.NsDepCop.Core.Util
{
    public static class AssemblyHelper
    {
        public static string GetDirectory(this Assembly assembly)
        {
            var codeBaseUri = new Uri(assembly.CodeBase);
            if (!codeBaseUri.IsFile)
                throw new Exception($"Unable to determine assembly folder because CodeBase is not a file Uri: {codeBaseUri}");

            var localPath = codeBaseUri.LocalPath;
            var assemblyFolder = Path.GetDirectoryName(localPath);
            if (assemblyFolder == null)
                throw new Exception($"Unable to determine assembly folder from local path: {codeBaseUri.LocalPath}");

            return assemblyFolder;
        }
    }
}
