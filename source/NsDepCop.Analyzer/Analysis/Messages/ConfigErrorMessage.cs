using System;

namespace Codartis.NsDepCop.Analysis.Messages
{
    /// <summary>
    /// A message that describes a config exception.
    /// </summary>
    public sealed class ConfigErrorMessage : AnalyzerMessageBase
    {
        public Exception Exception { get; }

        public ConfigErrorMessage(Exception exception)
        {
            Exception = exception;
        }
    }
}