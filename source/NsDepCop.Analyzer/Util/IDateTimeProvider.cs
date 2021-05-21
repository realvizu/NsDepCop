using System;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Provides date and time information.
    /// </summary>
    /// <remarks>
    /// Introduced for testability.
    /// </remarks>
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Returns the current UTC date and time.
        /// </summary>
        DateTime UtcNow { get; }
    }
}
