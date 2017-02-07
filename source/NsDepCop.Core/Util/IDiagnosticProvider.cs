using System;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// A type that provides diagnostic information.
    /// </summary>
    public interface IDiagnosticProvider
    {
        /// <summary>
        /// Gets or sets the callback used for reporting diagnostic messages.
        /// </summary>
        Action<string> DiagnosticMessageHandler { get; set; }
    }
}
