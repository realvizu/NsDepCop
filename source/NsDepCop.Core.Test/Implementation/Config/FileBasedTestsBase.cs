using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    /// <summary>
    /// Abstract base class for test classes that manipulate test files.
    /// </summary>
    public abstract class FileBasedTestsBase
    {
        protected string GetTestFilePath(string filename)
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.IsNotNull(assemblyDirectory);
            var path = Path.Combine(assemblyDirectory, @"Implementation\Config", this.GetType().Name, filename);
            return path;
        }

        protected static bool TryRename(string fromFilename, string toFilename)
        {
            try
            {
                File.Move(fromFilename, toFilename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected static bool TryDelete(string filename)
        {
            try
            {
                File.Delete(filename);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}