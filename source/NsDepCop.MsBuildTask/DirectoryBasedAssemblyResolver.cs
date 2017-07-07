using System;
using System.IO;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Resolves assemblies by trying to load them from a specified directory.
    /// </summary>
    public static class DirectoryBasedAssemblyResolver
    {
        private static string _directoryPath;

        static DirectoryBasedAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public static void Initialize(string directoryPath)
        {
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            var candidateAssemblyPath = Path.Combine(_directoryPath, assemblyName.Name + ".dll");

            return File.Exists(candidateAssemblyPath)
                ? Assembly.LoadFrom(candidateAssemblyPath) 
                : null;
        }
    }
}