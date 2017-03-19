using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Util
{
    public static class DumpHelper
    {
        public const int IndentSize = 2;

        public static void Dump(MessageHandler dumpMessageHandler, string message, int indent = 0)
        {
            dumpMessageHandler?.Invoke($"{new string(' ', indent * IndentSize)}{message}");
        }

        public static void Dump(MessageHandler dumpMessageHandler, IEnumerable<string> messages, int indent = 0)
        {
            foreach (var message in messages)
                Dump(dumpMessageHandler, message, indent);
        }
    }
}
