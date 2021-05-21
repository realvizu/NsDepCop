using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Util
{
    /// <summary>
    /// Static helper class that formats strings with indentation.
    /// </summary>
    public static class IndentHelper
    {
        public const int IndentSize = 2;

        public static IEnumerable<string> Indent(string message, int indent = 0)
            => Indent(new [] {message}, indent);

        public static IEnumerable<string> Indent(IEnumerable<string> messages, int indent = 0) 
            => messages.Select(i => Format(i, indent));

        private static string Format(string message, int indent = 0) 
            => $"{new string(' ', indent * IndentSize)}{message}";
    }
}
