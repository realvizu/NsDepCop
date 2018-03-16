using System;
using Codartis.NsDepCop.Core.Implementation.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer instances that call a remote (out-of-process) service.
    /// </summary>
    public static class RemoteDependencyAnalyzerFactory
    {
        public static IRemoteDependencyAnalyzer CreateClient(string serviceAddress, TimeSpan[] retryTimeSpans)
        {
            return new RemoteDependencyAnalyzerClient(serviceAddress, retryTimeSpans);
        }
    }
}
