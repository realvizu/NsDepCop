using System.Reflection;

namespace Codartis.NsDepCop.Core.Interface
{
    /// <summary>
    /// Product-wide constant values.
    /// </summary>
    public static class ProductConstants
    {
        public const string ToolName = "NsDepCop";
        public const string DefaultConfigFileName = "config.nsdepcop";
        public const string DisableToolEnvironmentVariableName = "DisableNsDepCop";

        public static readonly string Version = $"{Assembly.GetExecutingAssembly().GetName().Version}";
    }
}
