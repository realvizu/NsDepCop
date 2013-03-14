using Roslyn.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Represents the configuration of the tool.
    /// </summary>
    public class NsDepCopConfig
    {
        /// <summary>
        /// A value indicating whether analysis is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// A value representing the severity of an issue.
        /// </summary>
        public CodeIssueKind CodeIssueKind { get; private set; }

        /// <summary>
        /// A dictionary containing the allowed dependencies.
        /// </summary>
        /// <remarks>The key is the string representation of the dependency.</remarks>
        public Dictionary<string, Dependency> AllowedDependencies { get; private set; }

        /// <summary>
        /// Initializes a new instance with default values.
        /// </summary>
        /// <remarks>The analysis is disabled by default.</remarks>
        public NsDepCopConfig()
        {
            IsEnabled = Constants.DEFAULT_IS_ENABLED_VALUE;
            CodeIssueKind = Constants.DEFAULT_CODE_ISSUE_KIND;
            AllowedDependencies = new Dictionary<string, Dependency>();
        }

        /// <summary>
        /// Initializes a new instance with default values and a collection of dependencies.
        /// </summary>
        public NsDepCopConfig(IEnumerable<Dependency> allowedDependencies)
            : this()
        {
            allowedDependencies.EmptyIfNull().ToList()
                .ForEach(dependency => AllowedDependencies.Add(dependency.ToString(), dependency));
        }

        /// <summary>
        /// Initializes a new instance from a config file.
        /// </summary>
        public NsDepCopConfig(string configFilePath)
            : this()
        {
            LoadConfigFromFile(configFilePath);
        }

        /// <summary>
        /// Decides whether a dependency is allowed based on the rule configuration.
        /// </summary>
        /// <param name="fromNamespace">The namespace that depends on the other.</param>
        /// <param name="toNamespace">The namespace that the other namespace depends on.</param>
        /// <returns>True if the dependency is allowed, false otherwise.</returns>
        public bool IsAllowedDependency(string fromNamespace, string toNamespace)
        {
            // Generate all the possible patterns that can help match the namespaces with the rules.
            var fromPatterns = GenerateMatchingPatterns(fromNamespace);
            var toPatterns = GenerateMatchingPatterns(toNamespace);

            // Search for a match with the rules.
            Dependency foundDependency;
            foreach (var fromPattern in fromPatterns)
            {
                foreach (var toPattern in toPatterns)
                {
                    if (AllowedDependencies.TryGetValue(new Dependency(fromPattern, toPattern).ToString(), out foundDependency))
                    {
                        //Debug.WriteLine(string.Format("Dependency from '{0}' to '{1}' is allowed by rule '{2}'",
                        //    fromNamespace, toNamespace, foundDependency), Constants.TOOL_NAME);

                        // A matching rule was found.
                        return true;
                    }
                }
            }

            // No matching rule was found.
            return false;
        }

        /// <summary>
        /// Returns all the strings for the given namespace name that can match a dependency rule.
        /// </summary>
        /// <param name="namespaceName">A concrete namespace in string format.</param>
        /// <returns>All that string that can produce a match when searching matching rules.</returns>
        private List<string> GenerateMatchingPatterns(string namespaceName)
        {
            var result = new List<string>();
            // The '*' symbol means any namespace so it's always a matching pattern.
            result.Add("*");

            // Roslyn represents the global namespace like this: "<global namespace>".
            // This tool represents it like this: "."
            if (namespaceName.StartsWith("<"))
            {
                // If the namespace is the global namespace then substitute it with '.'
                result.Add(".");
            }
            else
            {
                // Add the concrete namespace name to the matching patterns.
                result.Add(namespaceName);

                // And add all the containing namespaces with a '*' (meaning sub-namespaces).
                // Eg. for 'System.Collections.Generic'
                // Add 'System.Collections.Generic.*'
                // Add 'System.Collections.*'
                // Add 'System.*'

                var pieces = namespaceName.Split('.');
                var prefix = "";

                foreach (var piece in pieces)
                {
                    if (prefix.Length > 0)
                        prefix += ".";

                    prefix += piece;
                    result.Add(prefix + ".*");
                }
            }

            return result;
        }

        /// <summary>
        /// Loads the configuration from a file.
        /// </summary>
        /// <param name="configFilePath">The config file with full path.</param>
        private void LoadConfigFromFile(string configFilePath)
        {
            // If no config file then analysis is switched off for the given project 
            // (by the default IsEnabled value which is false).
            if (!File.Exists(configFilePath))
                return;

            // Validate that it's an xml document and the root node is NsDepCopConfig.
            var configXml = XDocument.Load(configFilePath);
            if (configXml == null || configXml.Element("NsDepCopConfig") == null)
                throw new Exception(string.Format("Error in NsDepCop config file '{0}', NsDepCopConfig root element not found.", configFilePath));

            // Parse IsEnabled attribute, if exists.
            var isEnabledAttribute = configXml.Element("NsDepCopConfig").Attribute("IsEnabled");
            if (isEnabledAttribute != null)
            {
                bool isEnabled;
                if (bool.TryParse(isEnabledAttribute.Value, out isEnabled))
                {
                    IsEnabled = isEnabled;
                }
                else
                {
                    Debug.WriteLine(string.Format("Error parsing config file; IsEnabled attribute value '{0}'. Using default value:'{1}'.",
                        isEnabledAttribute.Value, IsEnabled), Constants.TOOL_NAME);
                }
            }

            // Parse CodeIssueKind attribute, if exists.
            var codeIssueKindAttribute = configXml.Element("NsDepCopConfig").Attribute("CodeIssueKind");
            if (codeIssueKindAttribute != null)
            {
                CodeIssueKind codeIssueKind;
                if (Enum.TryParse<CodeIssueKind>(codeIssueKindAttribute.Value, out codeIssueKind))
                {
                    CodeIssueKind = codeIssueKind;
                }
                else
                {
                    Debug.WriteLine(string.Format("Error parsing config file; CodeIssueKind attribute value '{0}'. Using default value:'{1}'.",
                        codeIssueKindAttribute.Value, CodeIssueKind), Constants.TOOL_NAME);
                }
            }

            // Parse Allowed elements.
            foreach (var xElement in configXml.Element("NsDepCopConfig").Elements("Allowed"))
            {
                var xAttributeFrom = xElement.Attribute("From");
                if (xAttributeFrom == null || xAttributeFrom.Value == null)
                {
                    Debug.WriteLine(string.Format("Error parsing config file, missing From attribute on element '{0}'. Ignoring element.",
                        xElement.Value), Constants.TOOL_NAME);
                    continue;
                }
                var xAttributeFromValue = xAttributeFrom.Value.Trim();
                if (xAttributeFromValue.Length == 0)
                {
                    Debug.WriteLine(string.Format("Error parsing config file, empty From attribute on element '{0}'. Ignoring element.",
                        xElement.Value), Constants.TOOL_NAME);
                    continue;
                }

                var xAttributeTo = xElement.Attribute("To");
                if (xAttributeTo == null || xAttributeTo.Value == null)
                {
                    Debug.WriteLine(string.Format("Error parsing config file, missing To attribute on element '{0}'. Ignoring element.",
                        xElement.Value), Constants.TOOL_NAME);
                    continue;
                }
                var xAttributeToValue = xAttributeTo.Value.Trim();
                if (xAttributeToValue.Length == 0)
                {
                    Debug.WriteLine(string.Format("Error parsing config file, empty To attribute on element '{0}'. Ignoring element.",
                        xElement.Value), Constants.TOOL_NAME);
                    continue;
                }

                var dependency = new Dependency(xAttributeFromValue, xAttributeToValue);
                AllowedDependencies.Add(dependency.ToString(), dependency);
            }
        }
    }
}
