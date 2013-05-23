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
        public const bool DEFAULT_IS_ENABLED_VALUE = false;
        public const IssueKind DEFAULT_ISSUE_KIND = IssueKind.Warning;
        public const int DEFAULT_MAX_ISSUE_REPORTED = 100;

        /// <summary>
        /// A value indicating whether analysis is enabled.
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// A value representing the severity of an issue.
        /// </summary>
        public IssueKind IssueKind { get; private set; }

        /// <summary>
        /// The max number of issues reported.
        /// </summary>
        public int MaxIssueCount { get; private set; }

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
            IsEnabled = DEFAULT_IS_ENABLED_VALUE;
            IssueKind = DEFAULT_ISSUE_KIND;
            MaxIssueCount = DEFAULT_MAX_ISSUE_REPORTED;
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
            if (configXml == null)
                throw new Exception(string.Format("Could not load NsDepCop config file '{0}'.", configFilePath));

            var rootElement = configXml.Element("NsDepCopConfig");
            if (rootElement == null)
                throw new Exception(string.Format("Error in NsDepCop config file '{0}', NsDepCopConfig root element not found.", configFilePath));

            // Parse attributes of the root node.
            IsEnabled = ParseAttribute(rootElement.Attribute("IsEnabled"), bool.TryParse, DEFAULT_IS_ENABLED_VALUE);
            IssueKind = ParseAttribute(rootElement.Attribute("CodeIssueKind"), Enum.TryParse<IssueKind>, DEFAULT_ISSUE_KIND);
            MaxIssueCount = ParseAttribute(rootElement.Attribute("MaxIssueCount"), int.TryParse, DEFAULT_MAX_ISSUE_REPORTED);

            // Parse Allowed elements.
            foreach (var xElement in rootElement.Elements("Allowed"))
            {
                string fromValue = ValidateAttribute(xElement, "From");
                if (fromValue == null)
                    continue;

                string toValue = ValidateAttribute(xElement, "To");
                if (toValue == null)
                    continue;

                // If the element was validated successfully then store the allowed dependency.
                var dependency = new Dependency(fromValue, toValue);
                AllowedDependencies.Add(dependency.ToString(), dependency);
            }
        }

        /// <summary>
        /// Validates an attribute of the given element that it occurs exactly once and has non-whitespace content.
        /// </summary>
        /// <param name="xElement">The parent element of the attribute to be validated.</param>
        /// <param name="attributeName">The name of the attribute to be validated.</param>
        /// <returns>The trimmed value of the attribute or null if the validation failed.</returns>
        private static string ValidateAttribute(XElement xElement, string attributeName)
        {
            var attributes = xElement.Attributes(attributeName);
            if (attributes.Count() != 1)
            {
                Debug.WriteLine(
                    string.Format("Error parsing config file: element '{0}' should have exactly 1 attribute named '{1}'. Ignoring element.",
                    xElement, attributeName),
                    Constants.TOOL_NAME);
                return null;
            }

            var attribute = attributes.First();
            if (attribute.Value == null)
            {
                Debug.WriteLine(
                    string.Format("Error parsing config file: element '{0}', attribute '{1}' has null value. Ignoring element.",
                    xElement, attributeName),
                    Constants.TOOL_NAME);
                return null;
            }

            var attributeValue = attribute.Value.Trim();
            if (attributeValue.Length == 0)
            {
                Debug.WriteLine(
                    string.Format("Error parsing config file: element '{0}', attribute '{1}' has whitespace value. Ignoring element.",
                    xElement, attributeName),
                    Constants.TOOL_NAME);
                return null;
            }

            return attributeValue;
        }

        /// <summary>
        /// Defines the signature of a TryParse-like method, that is used to parse a value of T from string.
        /// </summary>
        /// <typeparam name="T">The type of the parse result.</typeparam>
        /// <param name="s">The string that must be parsed.</param>
        /// <param name="t">The successfully parsed value.</param>
        /// <returns>True if successfully parsed, false otherwise.</returns>
        private delegate bool TryParseMethod<T>(string s, out T t);

        /// <summary>
        /// Parses an attribute to the given type. Returns the given default value if the parse failed.
        /// </summary>
        /// <typeparam name="T">The type of the parse result.</typeparam>
        /// <param name="attribute">An attribute object.</param>
        /// <param name="tryParseMethod">The method that should be used for parsing. Should not throw an exception just return false on failure.</param>
        /// <param name="defaultValue">The default value to be returned if the parse failed.</param>
        /// <returns>The parsed value or the given default value if the parse failed.</returns>
        private T ParseAttribute<T>(XAttribute attribute, TryParseMethod<T> tryParseMethod, T defaultValue)
        {
            T result = defaultValue;

            if (attribute != null)
            {
                T parseResult;
                if (tryParseMethod(attribute.Value, out parseResult))
                {
                    result = parseResult;
                }
                else
                {
                    Debug.WriteLine(string.Format("Error parsing config file: attribute name: '{0}', value: '{1}'. Using default value:'{2}'.",
                        attribute.Name, attribute.Value, defaultValue), Constants.TOOL_NAME);
                }
            }

            return result;
        }
    }
}