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

        private readonly string _serviceAddress;
        private readonly TimeSpan[] _retryTimeSpans;

        public DependencyAnalyzerClient(string serviceAddress, TimeSpan[] retryTimeSpans)
        {
            _serviceAddress = serviceAddress;
            _retryTimeSpans = retryTimeSpans ?? new TimeSpan[0];
        }

        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            return PerformCallWithRetries(i => i.AnalyzeProject(config, sourcePaths, referencedAssemblyPaths));
        }

        private TResult PerformCallWithRetries<TResult>(Func<IDependencyAnalyzerService, TResult> serviceOperation)
        {
            Exception lastException = null;
            var keepRetrying = true;
            var retryCount = 0;
            var maxRetryCount = _retryTimeSpans.Length;

            while (keepRetrying)
            {
                try
                {
                    var proxy = (IDependencyAnalyzerService)Activator.GetObject(typeof(IDependencyAnalyzerService), _serviceAddress);
                    return serviceOperation.Invoke(proxy);
                }
                catch (Exception e)
                {
                    lastException = e;

                    if (retryCount < maxRetryCount)
                    {
                        var retryTimeSpan = _retryTimeSpans[retryCount];
                        Trace.WriteLine($"[NsDepCop] {CommunicationErrorMessage} Trying to activate service and retrying after {retryTimeSpan}. Exception: {e.Message}");

                        AnalyzerServiceActivator.Activate();
                        Thread.Sleep(retryTimeSpan);

                        retryCount++;
                    }
                    else
                    {
                        keepRetrying = false;
                    }
                }
            }

            throw new Exception($"{CommunicationErrorMessage} Exception: {lastException}");
        }
    }
}
