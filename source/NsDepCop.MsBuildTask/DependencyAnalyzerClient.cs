using System;
using System.Diagnostics;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis.Service;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Client for calling dependency analyzer service via remoting.
    /// </summary>
    public class DependencyAnalyzerClient : IDependencyAnalyzerService
    {
        private const string CommunicationErrorMessage = "Unable to communicate with NsDepCop service.";

        private static readonly TimeSpan[] RetryTimeSpans =
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(1000),
            TimeSpan.FromMilliseconds(5000),
        };

        private readonly string _serviceAddress;

        public DependencyAnalyzerClient(string serviceAddress)
        {
            _serviceAddress = serviceAddress;
        }

        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            return PerformCallWithRetries(i => i.AnalyzeProject(config, sourcePaths, referencedAssemblyPaths));
        }

        private TResult PerformCallWithRetries<TResult>(Func<IDependencyAnalyzerService, TResult> serviceOperation)
        {
            Exception lastException = null;

            foreach (var retryTimeSpan in RetryTimeSpans)
            {
                try
                {
                    var proxy = (IDependencyAnalyzerService)Activator.GetObject(typeof(IDependencyAnalyzerService), _serviceAddress);
                    return serviceOperation.Invoke(proxy);
                }
                catch (Exception e)
                {
                    lastException = e;
                    Trace.WriteLine($"[NsDepCop] {CommunicationErrorMessage} Trying to activate service and retrying after {retryTimeSpan}. Exception: {e.Message}");
                    AnalyzerServiceActivator.Activate();
                    Thread.Sleep(retryTimeSpan);
                }
            }

            throw new Exception($"{CommunicationErrorMessage} Exception: {lastException}");
        }
    }
}
