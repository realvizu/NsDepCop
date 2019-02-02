using System;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.Core.Implementation
{
    public static class GlobalSettings
    {
        public static bool IsToolDisabled()
        {
            var environmentVariableValue = Environment.GetEnvironmentVariable(ProductConstants.DisableToolEnvironmentVariableName);

            return environmentVariableValue == "1" ||
                   string.Equals(environmentVariableValue, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
