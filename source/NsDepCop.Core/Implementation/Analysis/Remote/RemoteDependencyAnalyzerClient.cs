using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    /// <summary>
    /// Client for calling dependency analyzer service via remoting.
    /// </summary>
    public sealed class RemoteDependencyAnalyzerClient : IDependencyAnalyzer
    {
        private const string CommunicationErrorMessage = "Unable to communicate with NsDepCop service.";

        private readonly IAnalyzerConfig _config;
        private readonly string _serviceAddress;
        private readonly MessageHandler _traceMessageHandler;

        public RemoteDependencyAnalyzerClient(IAnalyzerConfig config, string serviceAddress, MessageHandler traceMessageHandler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _serviceAddress = serviceAddress ?? throw new ArgumentNullException(nameof(serviceAddress));
            _traceMessageHandler = traceMessageHandler;
        }

        public int HitCount { get; private set; }
        public int MissCount { get; private set; }
        public double EfficiencyPercent { get; private set; }

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var retryTimeSpans = _config.AnalyzerServiceCallRetryTimeSpans;
            var retryCount = 0;

            var retryResult = RetryHelper.Retry(
                () => InvokeRemoteAnalyzer(sourceFilePaths, referencedAssemblyPaths),
                retryTimeSpans.Length,
                e => ActivateServerAndWaitBeforeRetry(e, retryCount++, retryTimeSpans));

            return retryResult.Match(
                ProcessAnalyzerMessages,
                OnAllRetriesFailed);
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            throw new NotImplementedException();
        }

        private AnalyzerMessageBase[] InvokeRemoteAnalyzer(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            _traceMessageHandler?.Invoke("Calling analyzer service.");

            var proxy = (IRemoteDependencyAnalyzer)Activator.GetObject(typeof(IRemoteDependencyAnalyzer), _serviceAddress);
            var result = proxy.AnalyzeProject(_config, sourceFilePaths.ToArray(), referencedAssemblyPaths.ToArray());

            _traceMessageHandler?.Invoke("Calling analyzer service succeeded.");

            return result;
        }

        private void ActivateServerAndWaitBeforeRetry(Exception e, int retryCount, TimeSpan[] retryTimeSpans)
        {
            _traceMessageHandler?.Invoke($"{CommunicationErrorMessage} Exception: {e.Message}");

            _traceMessageHandler?.Invoke($"Trying to activate analyzer service (attempt #{retryCount + 1}).");
            AnalyzerServiceActivator.Activate(_traceMessageHandler);

            var sleepTimeSpan = retryTimeSpans[retryCount];
            _traceMessageHandler?.Invoke($"Retrying service call after: {sleepTimeSpan}.");
            Thread.Sleep(sleepTimeSpan);
        }

        private IEnumerable<TypeDependency> ProcessAnalyzerMessages(AnalyzerMessageBase[] analyzerMessages)
        {
            foreach (var analyzerMessage in analyzerMessages)
            {
                switch (analyzerMessage)
                {
                    case IllegalDependencyMessage illegalDependencyMessage:
                        yield return illegalDependencyMessage.IllegalDependency;
                        break;
                    case TraceMessage traceMessage:
                        _traceMessageHandler?.Invoke(traceMessage.Message);
                        break;
                    case CacheStatisticsMessage cacheStatisticsMessage:
                        HitCount = cacheStatisticsMessage.HitCount;
                        MissCount = cacheStatisticsMessage.MissCount;
                        EfficiencyPercent = cacheStatisticsMessage.EfficiencyPercent;
                        break;
                    default:
                        throw new Exception($"Unexpected {nameof(AnalyzerMessageBase)} descendant {analyzerMessage.GetType().Name}");
                }
            }
        }

        private static IEnumerable<TypeDependency> OnAllRetriesFailed(Exception exception)
        {
            throw new Exception($"{CommunicationErrorMessage} All retries failed.", exception);
        }
    }
}
