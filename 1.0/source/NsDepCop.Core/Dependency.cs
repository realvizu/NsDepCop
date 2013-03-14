using System;
using System.Diagnostics;
using System.Linq;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Represents a dependency between 2 namespaces.
    /// The 'From' namespace depends on the 'To' namespace.
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// The dependency points from this namespace to the other.
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// The dependency points into this namespace.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="from">The dependency points from this namespace to the other.</param>
        /// <param name="to">The dependency points into this namespace.</param>
        public Dependency(string from, string to)
        {
            From = from;
            To = to;
        }

        /// <summary>
        /// Returns the string represenation of a namespace dependency.
        /// </summary>
        /// <returns>The string represenation of a namespace dependency.</returns>
        public override string ToString()
        {
            return string.Format("{0}->{1}", From, To);
        }

        /// <summary>
        /// Converts a string to a Dependency if possible.
        /// </summary>
        /// <param name="ruleString">The string representation of a rule.</param>
        /// <returns>A dependency object or null if not succeeded.</returns>
        /// <remarks>
        /// The accepted string format is: FromNamespace->ToNamespace. 
        /// The following special notations can be used: 
        /// '.' is the global namespace, 
        /// '*' is any namespace.
        /// </remarks>
        public static Dependency Parse(string ruleString)
        {
            var stringParts = ruleString.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);

            if (stringParts.Length != 2)
            {
                Debug.WriteLine(string.Format("Error parsing dependency '{0}'.", ruleString), Constants.TOOL_NAME);
                return null;
            }

            var fromPart = stringParts[0].Trim();
            var toPart = stringParts[1].Trim();

            if (!IsValidNamespaceSpecification(fromPart))
            {
                Debug.WriteLine(string.Format("Error parsing dependency '{0}', 'From' part.", ruleString), Constants.TOOL_NAME);
                return null;
            }

            if (!IsValidNamespaceSpecification(toPart))
            {
                Debug.WriteLine(string.Format("Error parsing dependency '{0}', 'To' part.", ruleString), Constants.TOOL_NAME);
                return null;
            }

            return new Dependency(fromPart, toPart);
        }

        /// <summary>
        /// Validates that a string represents a namespace specification.
        /// </summary>
        /// <param name="namespaceSpecification">A namespace specification in string form.</param>
        /// <returns>True if the parameter is a namespace specification. False otherwise.</returns>
        /// <remarks>
        /// Valid namespace specifications are:
        /// <list type="bullet">
        /// <item>A concrete namespace, eg. 'System.IO'</item>
        /// <item>A concrete namespace and all subnamespaces, eg. 'System.IO.*'</item>
        /// <item>The global namespaces: '.'</item>
        /// <item>Any namespaces: '*'</item>
        /// </list>
        /// </remarks>
        private static bool IsValidNamespaceSpecification(string namespaceSpecification)
        {
            // "." means the global namespace, '*' means any namespace.
            if (namespaceSpecification == "." || namespaceSpecification == "*")
                return true;

            var pieces = namespaceSpecification.Split(new[] { '.' }, StringSplitOptions.None);
            return pieces.All(i => !string.IsNullOrWhiteSpace(i));
        }
    }

}
