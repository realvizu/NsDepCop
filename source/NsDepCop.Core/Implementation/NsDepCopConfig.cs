using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    /// <summary>
    /// Implements config file parsing.
    /// </summary>
    public class NsDepCopConfig : IRuleConfig
    {
        private const bool DEFAULT_IS_ENABLED_VALUE = true;
        private const IssueKind DEFAULT_ISSUE_KIND = IssueKind.Warning;
        private const int DEFAULT_MAX_ISSUE_REPORTED = 100;
        private const bool DEFAULT_CHILD_CAN_DEPEND_ON_PARENT_IMPLICITLY = false;

        private const string ROOT_ELEMENT_NAME = "NsDepCopConfig";
        private const string IS_ENABLED_ATTRIBUTE_NAME = "IsEnabled";
        private const string CODE_ISSUE_KIND_ATTRIBUTE_NAME = "CodeIssueKind";
        private const string MAX_ISSUE_COUNT_ATTRIBUTE_NAME = "MaxIssueCount";
        private const string IMPLICIT_PARENT_DEPENDENCY_ATTRIBUTE_NAME = "ChildCanDependOnParentImplicitly";
        private const string ALLOWED_ELEMENT_NAME = "Allowed";
        private const string DISALLOWED_ELEMENT_NAME = "Disallowed";
        private const string VISIBLE_MEMBERS_ELEMENT_NAME = "VisibleMembers";
        private const string TYPE_ELEMENT_NAME = "Type";
        private const string OF_NAMESPACE_ATTRIBUTE_NAME = "OfNamespace";
        private const string FROM_ATTRIBUTE_NAME = "From";
        private const string TO_ATTRIBUTE_NAME = "To";
        private const string TYPE_NAME_ATTRIBUTE_NAME = "Name";

        private readonly Dictionary<NamespaceDependencyRule, TypeNameSet> _allowedRulesBuilder;
        private readonly HashSet<NamespaceDependencyRule> _disallowedRulesBuilder;
        private readonly Dictionary<Namespace, TypeNameSet> _visibleTypesByNamespaceBuilder;

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
        /// True means that all child namespaces can depend on any of their parent namespaces without an explicit Allowed rule.
        /// True is in line with how C# type resolution works: it searches parent namespaces without an explicit using statement.
        /// False means that all dependencies must be explicitly allowed with a rule.
        /// False is the default for backward compatibility.
        /// </summary>
        public bool ChildCanDependOnParentImplicitly { get; private set; }

        /// <summary>
        /// Dictionary of allowed namespaces dependency rules. The key is a namespace dependency rule, 
        /// the value is a set of type names defined in the target namespace and visible for the source namespace(s).
        /// </summary>
        public ImmutableDictionary<NamespaceDependencyRule, TypeNameSet> AllowRules 
            => _allowedRulesBuilder.ToImmutableDictionary();

        /// <summary>
        /// The set of disallowed dependency rules.
        /// </summary>
        public ImmutableHashSet<NamespaceDependencyRule> DisallowRules 
            => _disallowedRulesBuilder.ToImmutableHashSet();

        /// <summary>
        /// Dictionary of visible types by target namespace. The Key is a namespace specification (must be singular), 
        /// the Value is a set of type names defined in the namespace and visible outside of the namespace.
        /// </summary>
        public ImmutableDictionary<Namespace, TypeNameSet> VisibleTypesByNamespace
            => _visibleTypesByNamespaceBuilder.ToImmutableDictionary();

        /// <summary>
        /// Initializes a new instance with default values.
        /// </summary>
        private NsDepCopConfig()
        {
            IsEnabled = DEFAULT_IS_ENABLED_VALUE;
            IssueKind = DEFAULT_ISSUE_KIND;
            MaxIssueCount = DEFAULT_MAX_ISSUE_REPORTED;

            _allowedRulesBuilder = new Dictionary<NamespaceDependencyRule, TypeNameSet>();
            _disallowedRulesBuilder = new HashSet<NamespaceDependencyRule>();
            _visibleTypesByNamespaceBuilder = new Dictionary<Namespace, TypeNameSet>();
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
            // If no config file then analysis is switched off for the given project.
            if (!File.Exists(configFilePath))
                return;

            try
            {
                var configXml = XDocument.Load(configFilePath, LoadOptions.SetLineInfo);

                var rootElement = configXml.Element(ROOT_ELEMENT_NAME);
                if (rootElement == null)
                    throw new Exception($"'{ROOT_ELEMENT_NAME}' root element not found.");

                ParseRootNodeAttributes(rootElement);
                ParseChildElements(rootElement);
            }
            catch (Exception e)
            {
                throw new Exception($"Error in '{configFilePath}': {e.Message}", e);
            }
        }

        private void ParseRootNodeAttributes(XElement rootElement)
        {
            IsEnabled = ParseAttribute(rootElement, IS_ENABLED_ATTRIBUTE_NAME, bool.TryParse, DEFAULT_IS_ENABLED_VALUE);
            IssueKind = ParseAttribute(rootElement, CODE_ISSUE_KIND_ATTRIBUTE_NAME, Enum.TryParse, DEFAULT_ISSUE_KIND);
            MaxIssueCount = ParseAttribute(rootElement, MAX_ISSUE_COUNT_ATTRIBUTE_NAME, int.TryParse, DEFAULT_MAX_ISSUE_REPORTED);
            ChildCanDependOnParentImplicitly = ParseAttribute(rootElement, IMPLICIT_PARENT_DEPENDENCY_ATTRIBUTE_NAME,
                bool.TryParse, DEFAULT_CHILD_CAN_DEPEND_ON_PARENT_IMPLICITLY);
        }

        private void ParseChildElements(XElement rootElement)
        {
            foreach (var xElement in rootElement.Elements())
            {
                switch (xElement.Name.ToString())
                {
                    case ALLOWED_ELEMENT_NAME:
                        ParseAllowedElement(xElement);
                        break;
                    case DISALLOWED_ELEMENT_NAME:
                        ParseDisallowedElement(xElement);
                        break;
                    case VISIBLE_MEMBERS_ELEMENT_NAME:
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

            var visibleMembersChild = xElement.Element(VISIBLE_MEMBERS_ELEMENT_NAME);
            if (visibleMembersChild != null)
            {
                if (allowedDependencyRule.To is NamespaceTree)
                    throw new Exception($"{GetLineInfo(xElement)}The target namespace '{allowedDependencyRule.To}' must be a single namespace.");

                if (visibleMembersChild.Attribute(OF_NAMESPACE_ATTRIBUTE_NAME) != null)
                    throw new Exception($"{GetLineInfo(xElement)}If {VISIBLE_MEMBERS_ELEMENT_NAME} is embedded in a dependency specification then '{OF_NAMESPACE_ATTRIBUTE_NAME}' attribute must not be defined.");

                visibleTypeNames = ParseTypeNameSet(visibleMembersChild, TYPE_ELEMENT_NAME);
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
            var targetNamespaceName = GetAttributeValue(xElement, OF_NAMESPACE_ATTRIBUTE_NAME);
            if (targetNamespaceName == null)
                throw new Exception($"{GetLineInfo(xElement)}'{OF_NAMESPACE_ATTRIBUTE_NAME}' attribute missing.");

            var targetNamespace = TryAndReportError(xElement, () => new Namespace(targetNamespaceName.Trim()));

            var visibleTypeNames = ParseTypeNameSet(xElement, TYPE_ELEMENT_NAME);
            if (!visibleTypeNames.Any())
                return;

            _visibleTypesByNamespaceBuilder.AddOrUnion<Namespace, TypeNameSet, string>(targetNamespace, visibleTypeNames);
        }

        private static NamespaceDependencyRule ParseDependencyRule(XElement xElement)
        {
            var fromValue = GetAttributeValue(xElement, FROM_ATTRIBUTE_NAME);
            if (fromValue == null)
                throw new Exception($"{GetLineInfo(xElement)}'{FROM_ATTRIBUTE_NAME}' attribute missing.");

            var toValue = GetAttributeValue(xElement, TO_ATTRIBUTE_NAME);
            if (toValue == null)
                throw new Exception($"{GetLineInfo(xElement)}'{TO_ATTRIBUTE_NAME}' attribute missing.");

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
                var typeName = GetAttributeValue(xElement, TYPE_NAME_ATTRIBUTE_NAME);
                if (typeName == null)
                    throw new Exception($"{GetLineInfo(xElement)}'{TYPE_NAME_ATTRIBUTE_NAME}' attribute missing.");

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