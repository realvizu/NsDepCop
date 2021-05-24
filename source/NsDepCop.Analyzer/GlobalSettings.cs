using System;

namespace Codartis.NsDepCop
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
