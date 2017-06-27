using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements assembly binding redirection by loading the executing dll's config file
    /// and hooking into the current AppDomain's AssemblyResolve events.
    /// </summary>
    public static class AssemblyBindingRedirector
    {
        private static readonly AssemblyBindingRedirectMap AssemblyBindingRedirectMap;

        static AssemblyBindingRedirector()
        {
            try
            {
                var executingAssemblyConfigPath = Assembly.GetExecutingAssembly().Location + ".config";
                var executingAssemblyConfigXml = LoadXml(executingAssemblyConfigPath);
                AssemblyBindingRedirectMap = AssemblyBindingRedirectMap.ParseXml(executingAssemblyConfigXml);

                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            }
            catch (Exception e)
            {
                // If binding redirect handling cannot be established then silently fail back to the host provided service.
                Trace.WriteLine($"AssemblyBindingRedirector ctor exception: {e}");
            }
        }

        public static void Initialize()
        {
            // Just to make sure that the static ctor was invoked.
        }

        /// <summary>
        /// Called when assembly resolution fails.
        /// Tries to load assemblies by applying the binding redirect info found in the executing assembly's config.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Assembly resolve event arguments.</param>
        /// <returns>The loaded assembly or null.</returns>
        private static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            var assemblyName = new AssemblyName(AppDomain.CurrentDomain.ApplyPolicy(e.Name));

            var redirectToVersion = AssemblyBindingRedirectMap.Find(assemblyName);
            if (redirectToVersion == null || redirectToVersion == assemblyName.Version)
                return null;

            assemblyName.Version = redirectToVersion;
            return Assembly.Load(assemblyName);
        }

        private static XDocument LoadXml(string xmlFilePath)
        {
            using (var stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return XDocument.Load(stream);
        }
    }
}