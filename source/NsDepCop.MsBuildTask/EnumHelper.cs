using System;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class EnumHelper
    {
        /// <summary>
        /// Parses a string to a nullable enum value. Returns null if a value could not be parsed.
        /// </summary>
        /// <typeparam name="TEnum">An enum type.</typeparam>
        /// <param name="valueAsString">The string to be parsed.</param>
        /// <returns>The parsed enum value or null.</returns>
        public static TEnum? ParseNullable<TEnum>(string valueAsString)
            where TEnum : struct
        {
            return Enum.TryParse(valueAsString, out TEnum result)
                ? (TEnum?)result
                : null;
        }
    }
}