using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Extracts config information from xml config files.
    /// </summary>
    internal sealed class XmlFileConfigProvider : FileConfigProviderBase
    {
        private const string RootElementName = "NsDepCopConfig";
        private const string IsEnabledAttributeName = "IsEnabled";
        private const string CodeIssueKindAttributeName = "CodeIssueKind";
        private const string MaxIssueCountAttributeName = "MaxIssueCount";
        private const string ImplicitParentDependencyAttributeName = "ChildCanDependOnParentImplicitly";
        private const string InfoImportanceAttributeName = "InfoImportance";
        private const string ParserAttributeName = "Parser";

        private const string AllowedElementName = "Allowed";
        private const string DisallowedElementName = "Disallowed";
        private const string VisibleMembersElementName = "VisibleMembers";
        private const string TypeElementName = "Type";
        private const string OfNamespaceAttributeName = "OfNamespace";
        private const string FromAttributeName = "From";
        private const string ToAttributeName = "To";
        private const string TypeNameAttributeName = "Name";

        private ProjectConfig _config;
        private Dictionary<NamespaceDependencyRule, TypeNameSet> _allowedRulesBuilder;
        private HashSet<NamespaceDependencyRule> _disallowedRulesBuilder;
        private Dictionary<Namespace, TypeNameSet> _visibleTypesByNamespaceBuilder;

        /// <summary>
        /// Initializes a new instance from a config file.
        /// </summary>
        public XmlFileConfigProvider(string configFilePath)
            : base(configFilePath)
        {
        }

        protected override IProjectConfig GetConfig()
        {
            _config = new ProjectConfig();
            _allowedRulesBuilder = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowedRulesBuilder = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByNamespaceBuilder = new Dictionary<Namespace, TypeNameSet>();

            try
            {
                var configXml = XDocument.Load(ConfigFilePath, LoadOptions.SetLineInfo);

                var rootElement = configXml.Element(RootElementName);
                if (rootElement == null)
                    throw new Exception($"'{RootElementName}' root element not found.");

                ParseRootNodeAttributes(rootElement);
                ParseChildElements(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception($"Error in '{ConfigFilePath}': {e.Message}", e);
            }

            _config.AllowRules = _allowedRulesBuilder.ToImmutableDictionary();
            _config.DisallowRules = _disallowedRulesBuilder.ToImmutableHashSet();
            _config.VisibleTypesByNamespace = _visibleTypesByNamespaceBuilder.ToImmutableDictionary();

            return _config;
        }

        private void ParseRootNodeAttributes(XElement rootElement)
        {
            _config.IsEnabled = ParseAttribute(rootElement, IsEnabledAttributeName, bool.TryParse, ConfigDefaults.IsEnabled);
            _config.IssueKind = ParseAttribute(rootElement, CodeIssueKindAttributeName, Enum.TryParse, ConfigDefaults.IssueKind);
            _config.MaxIssueCount = ParseAttribute(rootElement, MaxIssueCountAttributeName, int.TryParse, ConfigDefaults.MaxIssueReported);
            _config.ChildCanDependOnParentImplicitly = ParseAttribute(rootElement, ImplicitParentDependencyAttributeName,
                bool.TryParse, ConfigDefaults.ChildCanDependOnParentImplicitly);
            _config.InfoImportance = ParseAttribute(rootElement, InfoImportanceAttributeName, Enum.TryParse, ConfigDefaults.InfoImportance);
            _config.Parser = ParseAttribute(rootElement, ParserAttributeName, Enum.TryParse, ConfigDefaults.Parser);
        }

        private void ParseChildElements(XElement rootElement)
        {
            foreach (var xElement in rootElement.Elements())
            {
                switch (xElement.Name.ToString())
                {
                    case AllowedElementName:
                        ParseAllowedElement(xElement);
                        break;
                    case DisallowedElementName:
                        ParseDisallowedElement(xElement);
                        break;
                    case VisibleMembersElementName:
                        ParseVisibleMembersElement(xElement);
                        break;
                    default:
                        Trace.WriteLine($"Unexpected element '{xElement.Name}' ignored.");
                        break;
                }
            }
        }

        private void ParseAllowedElement(XElement xElement)
        {
            var allowedDependencyRule = ParseDependencyRule(xElement);

            TypeNameSet visibleTypeNames = null;

            var visibleMembersChild = xElement.Element(VisibleMembersElementName);
            if (visibleMembersChild != null)
            {
                if (allowedDependencyRule.To is NamespaceTree)
                    throw new Exception($"{GetLineInfo(xElement)}The target namespace '{allowedDependencyRule.To}' must be a single namespace.");

                if (visibleMembersChild.Attribute(OfNamespaceAttributeName) != null)
                    throw new Exception($"{GetLineInfo(xElement)}If {VisibleMembersElementName} is embedded in a dependency specification then '{OfNamespaceAttributeName}' attribute must not be defined.");

                visibleTypeNames = ParseTypeNameSet(visibleMembersChild, TypeElementName);
            }

            _allowedRulesBuilder.AddOrUnion<NamespaceDependencyRule, TypeNameSet, string>(allowedDependencyRule, visibleTypeNames);
        }

        private void ParseDisallowedElement(XElement xElement)
        {
            var disallowedDependencyRule = ParseDependencyRule(xElement);

            _disallowedRulesBuilder.Add(disallowedDependencyRule);
        }

        private void ParseVisibleMembersElement(XElement xElement)
        {
            var targetNamespaceName = GetAttributeValue(xElement, OfNamespaceAttributeName);
            if (targetNamespaceName == null)
                throw new Exception($"{GetLineInfo(xElement)}'{OfNamespaceAttributeName}' attribute missing.");

            var targetNamespace = TryAndReportError(xElement, () => new Namespace(targetNamespaceName.Trim()));

            var visibleTypeNames = ParseTypeNameSet(xElement, TypeElementName);
            if (!visibleTypeNames.Any())
                return;

            _visibleTypesByNamespaceBuilder.AddOrUnion<Namespace, TypeNameSet, string>(targetNamespace, visibleTypeNames);
        }

        private static NamespaceDependencyRule ParseDependencyRule(XElement xElement)
        {
            var fromValue = GetAttributeValue(xElement, FromAttributeName);
            if (fromValue == null)
                throw new Exception($"{GetLineInfo(xElement)}'{FromAttributeName}' attribute missing.");

            var toValue = GetAttributeValue(xElement, ToAttributeName);
            if (toValue == null)
                throw new Exception($"{GetLineInfo(xElement)}'{ToAttributeName}' attribute missing.");

            var fromNamespaceSpecification = TryAndReportError(xElement, () => NamespaceSpecificationParser.Parse(fromValue.Trim()));
            var toNamespaceSpecification = TryAndReportError(xElement, () => NamespaceSpecificationParser.Parse(toValue.Trim()));

            return new NamespaceDependencyRule(fromNamespaceSpecification, toNamespaceSpecification);
        }

        private static T TryAndReportError<T>(XObject xObject, Func<T> parserDelegate)
        {
            try
            {
                return parserDelegate();
            }
            catch (Exception e)
            {
                throw new Exception($"{GetLineInfo(xObject)}{e.Message}", e);
            }
        }

        private static TypeNameSet ParseTypeNameSet(XElement rootElement, string elementName)
        {
            var typeNameSet = new TypeNameSet();

            foreach (var xElement in rootElement.Elements(elementName))
            {
                var typeName = GetAttributeValue(xElement, TypeNameAttributeName);
                if (typeName == null)
                    throw new Exception($"{GetLineInfo(xElement)}'{TypeNameAttributeName}' attribute missing.");

                if (!string.IsNullOrWhiteSpace(typeName))
                    typeNameSet.Add(typeName.Trim());
            }

            return typeNameSet;
        }

        /// <summary>
        /// Returns an attribute's value, or null if the attribute was not found.
        /// </summary>
        /// <param name="xElement">The parent element of the attribute.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The value of the attribute or null if the attribute was not found.</returns>
        private static string GetAttributeValue(XElement xElement, string attributeName)
        {
            return xElement.Attribute(attributeName)?.Value;
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
        /// Parses an attribute of an element to the given type. 
        /// Returns the given default value if the attribute is not found.
        /// </summary>
        /// <typeparam name="T">The type of the parse result.</typeparam>
        /// <param name="element">The element where the attribute is searched.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="tryParseMethod">The method that should be used for parsing. Should return false on failure.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The parsed value or the given default value if the attribute is not found.</returns>
        private static T ParseAttribute<T>(XElement element, string attributeName, TryParseMethod<T> tryParseMethod, T defaultValue)
        {
            var result = defaultValue;

            var attribute = element.Attribute(attributeName);
            if (attribute != null)
            {
                T parseResult;
                if (tryParseMethod(attribute.Value, out parseResult))
                {
                    result = parseResult;
                }
                else
                {
                    throw new FormatException($"Error parsing '{attribute.Name}' value '{attribute.Value}'.");
                }
            }

            return result;
        }

        private static string GetLineInfo(XObject xObject)
        {
            var xmlLineInfo = xObject as IXmlLineInfo;

            return xmlLineInfo.HasLineInfo()
                ? $"[Line: {xmlLineInfo.LineNumber}, Pos: {xmlLineInfo.LinePosition}] "
                : string.Empty;
        }
    }
}