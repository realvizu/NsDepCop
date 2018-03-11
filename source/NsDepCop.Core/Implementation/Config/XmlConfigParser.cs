using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Parses a config provided in XML format.
    /// </summary>
    internal static class XmlConfigParser
    {
        private const string RootElementName = "NsDepCopConfig";
        private const string InheritanceDepthAttributeName = "InheritanceDepth";
        private const string IsEnabledAttributeName = "IsEnabled";
        private const string CodeIssueKindAttributeName = "CodeIssueKind";
        private const string MaxIssueCountAttributeName = "MaxIssueCount";
        private const string MaxIssueCountSeverityAttributeName = "MaxIssueCountSeverity";
        private const string ImplicitParentDependencyAttributeName = "ChildCanDependOnParentImplicitly";
        private const string InfoImportanceAttributeName = "InfoImportance";
        private const string AllowedElementName = "Allowed";
        private const string DisallowedElementName = "Disallowed";
        private const string VisibleMembersElementName = "VisibleMembers";
        private const string TypeElementName = "Type";
        private const string OfNamespaceAttributeName = "OfNamespace";
        private const string FromAttributeName = "From";
        private const string ToAttributeName = "To";
        private const string TypeNameAttributeName = "Name";

        public static AnalyzerConfigBuilder Parse(XDocument configXml)
        {
            var configBuilder = new AnalyzerConfigBuilder();

            var rootElement = configXml.Element(RootElementName);
            if (rootElement == null)
                throw new Exception($"'{RootElementName}' root element not found.");

            ParseRootNodeAttributes(rootElement, configBuilder);
            ParseChildElements(rootElement, configBuilder);

            return configBuilder;
        }

        private static void ParseRootNodeAttributes(XElement rootElement, AnalyzerConfigBuilder configBuilder)
        {
            configBuilder.SetIsEnabled(ParseAttribute<bool>(rootElement, IsEnabledAttributeName, bool.TryParse));
            configBuilder.SetInheritanceDepth(ParseAttribute<int>(rootElement, InheritanceDepthAttributeName, int.TryParse));
            configBuilder.SetIssueKind(ParseAttribute<IssueKind>(rootElement, CodeIssueKindAttributeName, Enum.TryParse));
            configBuilder.SetInfoImportance(ParseAttribute<Importance>(rootElement, InfoImportanceAttributeName, Enum.TryParse));
            configBuilder.SetChildCanDependOnParentImplicitly(ParseAttribute<bool>(rootElement, ImplicitParentDependencyAttributeName, bool.TryParse));
            configBuilder.SetMaxIssueCount(ParseAttribute<int>(rootElement, MaxIssueCountAttributeName, int.TryParse));
            configBuilder.SetMaxIssueCountSeverity(ParseAttribute<IssueKind>(rootElement, MaxIssueCountSeverityAttributeName, Enum.TryParse));
        }

        private static void ParseChildElements(XElement rootElement, AnalyzerConfigBuilder configBuilder)
        {
            foreach (var xElement in rootElement.Elements())
            {
                switch (xElement.Name.ToString())
                {
                    case AllowedElementName:
                        ParseAllowedElement(xElement, configBuilder);
                        break;
                    case DisallowedElementName:
                        ParseDisallowedElement(xElement, configBuilder);
                        break;
                    case VisibleMembersElementName:
                        ParseVisibleMembersElement(xElement, configBuilder);
                        break;
                    default:
                        Trace.WriteLine($"Unexpected element '{xElement.Name}' ignored.");
                        break;
                }
            }
        }

        private static void ParseAllowedElement(XElement xElement, AnalyzerConfigBuilder configBuilder)
        {
            var allowedDependencyRule = ParseDependencyRule(xElement);

            var visibleTypeNames = ParseVisibleMembersInsideAllowedRule(xElement, allowedDependencyRule);
            if (visibleTypeNames.IsNullOrEmpty())
                visibleTypeNames = null;

            configBuilder.AddAllowRule(allowedDependencyRule, visibleTypeNames);
        }

        private static void ParseDisallowedElement(XElement xElement, AnalyzerConfigBuilder configBuilder)
        {
            var disallowedDependencyRule = ParseDependencyRule(xElement);

            configBuilder.AddDisallowRule(disallowedDependencyRule);
        }

        private static TypeNameSet ParseVisibleMembersInsideAllowedRule(XElement xElement, NamespaceDependencyRule allowedRule)
        {
            var visibleMembersChild = xElement.Element(VisibleMembersElementName);
            if (visibleMembersChild == null)
                return null;

            if (allowedRule.To is NamespaceTree)
                throw new Exception($"{GetLineInfo(xElement)}The target namespace '{allowedRule.To}' must be a single namespace.");

            if (visibleMembersChild.Attribute(OfNamespaceAttributeName) != null)
                throw new Exception($"{GetLineInfo(xElement)}If {VisibleMembersElementName} is embedded in a dependency specification then '{OfNamespaceAttributeName}' attribute must not be defined.");

            return ParseTypeNameSet(visibleMembersChild, TypeElementName);
        }

        private static void ParseVisibleMembersElement(XElement xElement, AnalyzerConfigBuilder configBuilder)
        {
            var targetNamespaceName = GetAttributeValue(xElement, OfNamespaceAttributeName);
            if (targetNamespaceName == null)
                throw new Exception($"{GetLineInfo(xElement)}'{OfNamespaceAttributeName}' attribute missing.");

            var targetNamespace = TryAndReportError(xElement, () => new Namespace(targetNamespaceName.Trim()));

            var visibleTypeNames = ParseTypeNameSet(xElement, TypeElementName);
            if (!visibleTypeNames.Any())
                return;

            configBuilder.AddVisibleTypesByNamespace(targetNamespace, visibleTypeNames);
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
        /// Returns null if the attribute is not found.
        /// </summary>
        /// <typeparam name="T">The type of the parse result.</typeparam>
        /// <param name="element">The element where the attribute is searched.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="tryParseMethod">The method that should be used for parsing. Should return false on failure.</param>
        /// <returns>The parsed value or null if the attribute is not found.</returns>
        private static T? ParseAttribute<T>(XElement element, string attributeName, TryParseMethod<T> tryParseMethod)
            where T : struct 
        {
            var attribute = element.Attribute(attributeName);
            if (attribute == null)
                return null;

            if (tryParseMethod(attribute.Value, out var parseResult))
                return parseResult;

            throw new FormatException($"{GetLineInfo(element)}Error parsing '{attribute.Name}' value '{attribute.Value}'.");
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