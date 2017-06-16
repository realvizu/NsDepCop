using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Stores and retrieves assembly binding redirect information.
    /// Parses assembly redirect info from config xml.
    /// </summary>
    internal class AssemblyBindingRedirectMap
    {
        private static readonly XNamespace AssemblyBindingNamespace = XNamespace.Get("urn:schemas-microsoft-com:asm.v1");

        private readonly IDictionary<AssemblyIdentity, VersionRedirect> _redirectMap;

        private AssemblyBindingRedirectMap(IDictionary<AssemblyIdentity, VersionRedirect> redirectMap)
        {
            _redirectMap = redirectMap ?? throw new ArgumentNullException(nameof(redirectMap));
        }

        public Version Find(AssemblyName assemblyName)
        {
            var assemblyIdentity = new AssemblyIdentity(assemblyName);
            return _redirectMap.TryGetValue(assemblyIdentity, out VersionRedirect versionRedirect)
                   && versionRedirect.Match(assemblyName.Version)
                ? versionRedirect.NewVersion
                : null;
        }

        public static AssemblyBindingRedirectMap ParseXml(XDocument xDocument)
        {
            var map = new Dictionary<AssemblyIdentity, VersionRedirect>();

            foreach (var dependentAssemblyElement in GetDependentAssemblyElements(xDocument))
            {
                var assemblyIndentityElement = GetElementOrThrow(dependentAssemblyElement, AssemblyBindingNamespace + "assemblyIdentity");

                var assemblyName = GetAttributeValueOrThrow(assemblyIndentityElement, "name");
                var assemblyPublicKeyToken = GetAttributeValueOrThrow(assemblyIndentityElement, "publicKeyToken");
                var assemblyCulture = GetAttributeValueOrThrow(assemblyIndentityElement, "culture");

                var bindingRedirectElement = GetElementOrThrow(dependentAssemblyElement, AssemblyBindingNamespace + "bindingRedirect");

                var oldVersionString = GetAttributeValueOrThrow(bindingRedirectElement, "oldVersion");
                var oldVersionInterval = oldVersionString.Split('-').Select(Version.Parse).ToList();

                var newVersionString = GetAttributeValueOrThrow(bindingRedirectElement, "newVersion");
                var newVersion = Version.Parse(newVersionString);

                var assemblyIdentity = new AssemblyIdentity(assemblyName, assemblyPublicKeyToken, assemblyCulture);
                var assemblyVersionRedirection = new VersionRedirect(oldVersionInterval[0], oldVersionInterval[1], newVersion);

                map.Add(assemblyIdentity, assemblyVersionRedirection);
            }

            return new AssemblyBindingRedirectMap(map);
        }

        private static IEnumerable<XElement> GetDependentAssemblyElements(XDocument xDocument)
        {
            return xDocument
                ?.Element("configuration")
                ?.Element("runtime")
                ?.Element(AssemblyBindingNamespace + "assemblyBinding")
                ?.Elements(AssemblyBindingNamespace + "dependentAssembly")
                ?? Enumerable.Empty<XElement>();
        }

        private static XElement GetElementOrThrow(XElement parentElement, XName childElementName)
            => parentElement.Element(childElementName) ?? throw new Exception($"'{childElementName}' element not found.");

        private static string GetAttributeValueOrThrow(XElement element, string attributeName) 
            => element.Attribute(attributeName)?.Value ?? throw new Exception($"'{attributeName}' attribute not found.");
    }
}
