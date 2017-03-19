using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Provides diagnostic support operations.
    /// </summary>
    public interface IDiagnosticSupport
    {
        /// <summary>
        /// Returns the state of the object as a string collection
        /// </summary>
        IEnumerable<string> ToStrings();
    }
}
