using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Codartis.NsDepCop.TestUtil
{
    /// <summary>
    /// Abstract base class for test classes that manipulate test files.
    /// </summary>
    public abstract class FileBasedTestsBase
    {
        protected static string GetExecutingAssemblyDirectory() => GetAssemblyDirectory(Assembly.GetExecutingAssembly());

        protected static string GetAssemblyPath(Assembly assembly)
        {
            return assembly.Location;
        }

        protected static string GetAssemblyDirectory(Assembly assembly)
        {
            var assemblyPath = GetAssemblyPath(assembly);
            return Path.GetDirectoryName(assemblyPath);
        }

        protected static string GetBinFilePath(string filename)
        {
            return Path.Combine(GetExecutingAssemblyDirectory(), filename);
        }

        protected string GetFilePathInTestClassFolder(string filename)
        {
            var namespacePrefix = $"Codartis.{this.GetType().Assembly.GetName().Name}";
            var namespacePostfix = GetType().FullName.Remove(0, namespacePrefix.Length + 1).Replace('.', '\\');

            return GetBinFilePath(Path.Combine(namespacePostfix, filename));
        }

        protected string LoadFile(string fullPath)
        {
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var streamReader = new StreamReader(stream))
            {
                return streamReader.ReadToEnd();
            }
        }

        protected static void Rename(string fromFilename, string toFilename)
        {
            if (File.Exists(fromFilename))
            {
                if (File.Exists(toFilename))
                    throw new InvalidOperationException($"Cannot rename '{fromFilename}' to '{toFilename}' because it already exists.");

                File.Move(fromFilename, toFilename);
            }
        }

        protected static void Delete(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            while (File.Exists(filename))
                Thread.Sleep(100);
        }
    }
}