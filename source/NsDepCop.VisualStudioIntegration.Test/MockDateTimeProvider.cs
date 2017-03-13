using System;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration.Test
{
    /// <summary>
    /// Implements a date time provider for testing purposes.
    /// </summary>
    public class MockDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow { get; set; }
    }
}
