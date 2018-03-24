namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// Provides the address of the remote dependency analyzer service.
    /// </summary>
    public static class ServiceAddressProvider
    {
        public const string ServiceHostProcessName = "NsDepCop.ServiceHost";
        public static readonly string PipeName = $"{ProductConstants.ToolName}-{ProductConstants.Version}";
        public static readonly string ServiceName = $"{nameof(IRemoteDependencyAnalyzer)}-{ProductConstants.Version}";
        public static readonly string ServiceAddress = $"ipc://{PipeName}/{ServiceName}";
    }
}
