using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements assembly binding redirection by loading the executing dll's config file
    /// and hooking into the current AppDomain's AssemblyResolve events.
    /// </summary>
    public class AssemblyBindingRedirector
    {
        private readonly MessageHandler _traceMessageHandler;
        private AssemblyBindingRedirectMap _assemblyBindingRedirectMap;

        public AssemblyBindingRedirector(MessageHandler traceMessageHandler)
        {
            _traceMessageHandler = traceMessageHandler;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                var executingAssemblyConfigPath = Assembly.GetExecutingAssembly().Location + ".config";
                var executingAssemblyConfigXml = LoadXml(executingAssemblyConfigPath);
                _assemblyBindingRedirectMap = AssemblyBindingRedirectMap.ParseXml(executingAssemblyConfigXml);

                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssembly;
                AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            }
            catch (Exception e)
            {
                // If binding redirect handling cannot be established then silently fail back to the host provided service.
                _traceMessageHandler?.Invoke(new[] { e.ToString() });
            }
        }

        /// <summary>
        /// Called when assembly resolution fails.
        /// Tries to load assemblies by applying the binding redirect info found in the executing assembly's config.
        /// </summary>
        /// <param name="sender">Unused.</param>
        /// <param name="e">Assembly resolve event arguments.</param>
        /// <returns>The loaded assembly or null.</returns>
        private Assembly ResolveAssembly(object sender, ResolveEventArgs e)
        {
            var assemblyName = new AssemblyName(e.Name);

            var redirectToVersion = _assemblyBindingRedirectMap.Find(assemblyName);
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