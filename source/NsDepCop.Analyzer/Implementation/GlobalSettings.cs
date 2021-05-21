using System;
using Codartis.NsDepCop.Interface;

namespace Codartis.NsDepCop.Implementation
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
