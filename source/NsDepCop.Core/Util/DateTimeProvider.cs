using System;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Implements a date and time provider with the system date and time.
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
