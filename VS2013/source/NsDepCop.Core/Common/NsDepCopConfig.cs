using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Codartis.NsDepCop.Core.Common
{
    /// <summary>
    /// Represents the configuration of the tool.
    /// </summary>
    public class NsDepCopConfig
    {
        private const bool DEFAULT_IS_ENABLED_VALUE = false;
        private const IssueKind DEFAULT_ISSUE_KIND = IssueKind.Warning;
        private const int DEFAULT_MAX_ISSUE_REPORTED = 100;

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
        /// The set of allowed dependencies.
        /// </summary>
        public ImmutableHashSet<Dependency> AllowedDependencies { get; private set; }

        /// <summary>
        /// The set of disallowed dependencies.
        /// </summary>
        public ImmutableHashSet<Dependency> DisallowedDependencies { get; private set; }

        /// <summary>
        /// Initializes a new instance with default values.
        /// </summary>
        private NsDepCopConfig()
        {
            IsEnabled = DEFAULT_IS_ENABLED_VALUE;
            IssueKind = DEFAULT_ISSUE_KIND;
            MaxIssueCount = DEFAULT_MAX_ISSUE_REPORTED;
            AllowedDependencies = ImmutableHashSet.Create<Dependency>();
            DisallowedDependencies = ImmutableHashSet.Create<Dependency>();
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
            IssueKind = ParseAttribute(rootElement.Attribute("CodeIssueKind"), Enum.TryParse, DEFAULT_ISSUE_KIND);
            MaxIssueCount = ParseAttribute(rootElement.Attribute("MaxIssueCount"), int.TryParse, DEFAULT_MAX_ISSUE_REPORTED);

            AllowedDependencies = BuildDependencySet(rootElement, "Allowed");
            DisallowedDependencies = BuildDependencySet(rootElement, "Disallowed");
        }

        /// <summary>
        /// Builds an immutable hashset containing dependencies parsed from the given XElement root with the given element name. 
        /// </summary>
        /// <param name="rootElement">Root of elements to be parsed.</param>
        /// <param name="elementName">The name of the elements to be parsed.</param>
        /// <returns>An immutable set of parsed dependencies.</returns>
        private static ImmutableHashSet<Dependency> BuildDependencySet(XElement rootElement, string elementName)
        {
            var builder = ImmutableHashSet.CreateBuilder<Dependency>();

            foreach (var xElement in rootElement.Elements(elementName))
            {
                var dependency = ParseDependency(xElement);
                if (dependency != null && !builder.Contains(dependency))
                    builder.Add(dependency);
            }

            return builder.ToImmutable();
        }

        /// <summary>
        /// Parse an XEelement that defines a dependency.
        /// </summary>
        /// <param name="xElement">An xml element.</param>
        /// <returns>The parsed dependency, or null if could not parse.</returns>
        private static Dependency ParseDependency(XElement xElement)
        {
            Dependency dependency = null;

            try
            {
                var fromValue = GetAttributeValue(xElement, "From");
                if (fromValue == null)
                    throw new Exception("From element missing.");

                var toValue = GetAttributeValue(xElement, "To");
                if (toValue == null)
                    throw new Exception("To element missing.");

                dependency = new Dependency(fromValue, toValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(
                    string.Format("Error parsing config file: element '{0}' is invalid. ({1}) Ignoring element.",
                    xElement, e.Message),
                    Constants.TOOL_NAME);
            }

            return dependency;
        }

        /// <summary>
        /// Returns an attribute's value, or null if the attribute was not found.
        /// </summary>
        /// <param name="xElement">The parent element of the attribute.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The value of the attribute or null if the attribute was not found.</returns>
        private static string GetAttributeValue(XElement xElement, string attributeName)
        {
            var attribute = xElement.Attribute(attributeName);
            return attribute == null ? null : attribute.Value;
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
        private static T ParseAttribute<T>(XAttribute attribute, TryParseMethod<T> tryParseMethod, T defaultValue)
        {
            var result = defaultValue;

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