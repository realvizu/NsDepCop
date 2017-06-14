using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// A delegate that receives string messages.
    /// </summary>
    /// <param name="messages">A collection of string messages.</param>
    public delegate void MessageHandler(IEnumerable<string> messages);
}
