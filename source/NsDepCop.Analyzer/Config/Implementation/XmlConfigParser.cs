using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Parses a config provided in XML format.
    /// </summary>
    public static class XmlConfigParser
    {
        private const string RootElementName = "NsDepCopConfig";
        private const string InheritanceDepthAttributeName = "InheritanceDepth";
        private const string IsEnabledAttributeName = "IsEnabled";
        private const string MaxIssueCountAttributeName = "MaxIssueCount";
        private const string AutoLowerMaxIssueCountAttributeName = "AutoLowerMaxIssueCount";
        private const string ImplicitParentDependencyAttributeName = "ChildCanDependOnParentImplicitly";
        private const string ImplicitChildDependencyAttributeName = "ParentCanDependOnChildImplicitly";
        private const string SourcePathExclusionPatternsAttributeName = "ExcludedFiles";
        private const string CheckAssemblyDependenciesAttributeName = "CheckAssemblyDependencies";
        private const string AllowedElementName = "Allowed";
        private const string DisallowedElementName = "Disallowed";
        private const string AllowedAssemblyElementName = "AllowedAssembly";
        private const string DisallowedAssemblyElementName = "DisallowedAssembly";
        private const string VisibleMembersElementName = "VisibleMembers";
        private const string TypeElementName = "Type";
        private const string OfNamespaceAttributeName = "OfNamespace";
        private const string FromAttributeName = "From";
        private const string ToAttributeName = "To";
        private const string TypeNameAttributeName = "Name";

        public static AnalyzerConfigBuilder Parse(XDocument configXml)
        {
            var configBuilder = new AnalyzerConfigBuilder();

            var rootElement = GetRootElement(configXml);
            ParseRootNodeAttributes(rootElement, configBuilder);
            ParseChildElements(rootElement, configBuilder);

            return configBuilder;
        }

        public static void UpdateMaxIssueCount(XDocument configXml, int newValue)
        {
            var rootElement = GetRootElement(configXml);

            AddOrUpdateAttribute(rootElement, MaxIssueCountAttributeName, newValue.ToString());
        }

        private static XElement GetRootElement(XDocument configXml)
        {
            var rootElement = configXml.Element(RootElementName);
            if (rootElement == null)
                throw new Exception($"'{RootElementName}' root element not found.");
            return rootElement;
        }

        private static void ParseRootNodeAttributes(XElement rootElement, AnalyzerConfigBuilder configBuilder)
        {
            configBuilder.SetIsEnabled(ParseValueType<bool>(rootElement, IsEnabledAttributeName, bool.TryParse));
            configBuilder.SetInheritanceDepth(ParseValueType<int>(rootElement, InheritanceDepthAttributeName, int.TryParse));
            configBuilder.AddSourcePathExclusionPatterns(ParseStringList(rootElement, SourcePathExclusionPatternsAttributeName, ','));
            configBuilder.SetCheckAssemblyDependencies(ParseValueType<bool>(rootElement, CheckAssemblyDependenciesAttributeName, bool.TryParse));
            configBuilder.SetChildCanDependOnParentImplicitly(ParseValueType<bool>(rootElement, ImplicitParentDependencyAttributeName, bool.TryParse));
            configBuilder.SetParentCanDependOnChildImplicitly(ParseValueType<bool>(rootElement, ImplicitChildDependencyAttributeName, bool.TryParse));
            configBuilder.SetMaxIssueCount(ParseValueType<int>(rootElement, MaxIssueCountAttributeName, int.TryParse));
            configBuilder.SetAutoLowerMaxIssueCount(ParseValueType<bool>(rootElement, AutoLowerMaxIssueCountAttributeName, bool.TryParse));
        }

        private static IEnumerable<string> ParseStringList(XElement element, string attributeName, char separatorChar)
        {
            var attribute = element.Attribute(attributeName);
            var parts = Split(attribute?.Value, separatorChar);
            return parts?.ToList();
        }

        private static IEnumerable<string> Split(string s, char separatorChar)
        {
            return s?.Split(new[] {separatorChar}, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim());
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
                    case AllowedAssemblyElementName:
                        ParseAllowedAssemblyElement(xElement, configBuilder);
                        break;
                    case DisallowedAssemblyElementName:
                        ParseDisallowedAssemblyElement(xElement, configBuilder);
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

        private static void ParseAllowedElement(XElement element, AnalyzerConfigBuilder configBuilder)
        {
            var allowedDependencyRule = ParseDependencyRule(element);

            var visibleTypeNames = ParseVisibleMembersInsideAllowedRule(element, allowedDependencyRule);
            if (visibleTypeNames.IsNullOrEmpty())
                visibleTypeNames = null;

            configBuilder.AddAllowRule(allowedDependencyRule, visibleTypeNames);
        }

        private static void ParseDisallowedElement(XElement element, AnalyzerConfigBuilder configBuilder)
        {
            var disallowedDependencyRule = ParseDependencyRule(element);

            configBuilder.AddDisallowRule(disallowedDependencyRule);
        }

        private static void ParseAllowedAssemblyElement(XElement element, AnalyzerConfigBuilder configBuilder)
        {
            var allowedAssemblyDependencyRule = ParseAssemblyDependencyRule(element);

            configBuilder.AddAllowedAssemblyRule(allowedAssemblyDependencyRule);
        }

        private static void ParseDisallowedAssemblyElement(XElement element, AnalyzerConfigBuilder configBuilder)
        {
            var disallowedAssemblyDependencyRule = ParseAssemblyDependencyRule(element);

            configBuilder.AddDisallowedAssemblyRule(disallowedAssemblyDependencyRule);
        }

        private static TypeNameSet ParseVisibleMembersInsideAllowedRule(XElement element, DependencyRule allowedRule)
        {
            var visibleMembersChild = element.Element(VisibleMembersElementName);
            if (visibleMembersChild == null)
                return null;

            if (allowedRule.To is not Domain)
                throw new Exception($"{GetLineInfo(element)}The target namespace '{allowedRule.To}' must be a single namespace.");

            if (visibleMembersChild.Attribute(OfNamespaceAttributeName) != null)
                throw new Exception(
                    $"{GetLineInfo(element)}If {VisibleMembersElementName} is embedded in a dependency specification then '{OfNamespaceAttributeName}' attribute must not be defined.");

            return ParseTypeNameSet(visibleMembersChild, TypeElementName);
        }

        private static void ParseVisibleMembersElement(XElement element, AnalyzerConfigBuilder configBuilder)
        {
            var targetNamespaceName = GetAttributeValue(element, OfNamespaceAttributeName);
            if (targetNamespaceName == null)
                throw new Exception($"{GetLineInfo(element)}'{OfNamespaceAttributeName}' attribute missing.");

            var targetNamespace = TryAndReportError(element, () => new Domain(targetNamespaceName.Trim()));

            var visibleTypeNames = ParseTypeNameSet(element, TypeElementName);
            if (!visibleTypeNames.Any())
                return;

            configBuilder.AddVisibleTypesByNamespace(targetNamespace, visibleTypeNames);
        }

        private static DependencyRule ParseDependencyRule(XElement element)
        {
            var fromValue = GetAttributeValue(element, FromAttributeName);
            if (fromValue == null)
                throw new Exception($"{GetLineInfo(element)}'{FromAttributeName}' attribute missing.");

            var toValue = GetAttributeValue(element, ToAttributeName);
            if (toValue == null)
                throw new Exception($"{GetLineInfo(element)}'{ToAttributeName}' attribute missing.");

            var fromNamespaceSpecification = TryAndReportError(element, () => DomainSpecificationParser.Parse(fromValue.Trim()));
            var toNamespaceSpecification = TryAndReportError(element, () => DomainSpecificationParser.Parse(toValue.Trim()));

            return new DependencyRule(fromNamespaceSpecification, toNamespaceSpecification);
        }

        private static DependencyRule ParseAssemblyDependencyRule(XElement element)
        {
            var fromValue = GetAttributeValue(element, FromAttributeName);
            if (fromValue == null)
                throw new Exception($"{GetLineInfo(element)}'{FromAttributeName}' attribute missing.");

            var toValue = GetAttributeValue(element, ToAttributeName);
            if (toValue == null)
                throw new Exception($"{GetLineInfo(element)}'{ToAttributeName}' attribute missing.");

            var fromNamespaceSpecification = TryAndReportError(element, () => DomainSpecificationParser.Parse(fromValue.Trim()));
            var toNamespaceSpecification = TryAndReportError(element, () => DomainSpecificationParser.Parse(toValue.Trim()));

            return new DependencyRule(fromNamespaceSpecification, toNamespaceSpecification);
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

        private static TypeNameSet ParseTypeNameSet(XElement element, string childElementName)
        {
            var typeNameSet = new TypeNameSet();

            foreach (var xElement in element.Elements(childElementName))
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
        /// <param name="element">The parent element of the attribute.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>The value of the attribute or null if the attribute was not found.</returns>
        private static string GetAttributeValue(XElement element, string attributeName)
        {
            return element.Attribute(attributeName)?.Value;
        }

        private static void AddOrUpdateAttribute(XElement element, string attributeName, string newValue)
        {
            var attribute = element.Attribute(attributeName);

            if (attribute == null)
                element.Add(new XAttribute(attributeName, newValue));
            else
                attribute.Value = newValue;
        }

        /// <summary>
        /// Defines the signature of a TryParse-like method, that is used to parse a value of T from string.
        /// </summary>
        /// <typeparam name="T">The type of the parse result.</typeparam>
        /// <param name="s">The string that must be parsed.</param>
        /// <param name="t">The successfully parsed value.</param>
        /// <returns>True if successfully parsed, false otherwise.</returns>
        private delegate bool TryParseMethod<T>(string s, out T t);

        private static T? ParseValueType<T>(XElement element, string attributeName, TryParseMethod<T> tryParseMethod)
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