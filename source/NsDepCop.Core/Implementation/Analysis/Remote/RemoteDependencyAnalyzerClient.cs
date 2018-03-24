using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _config = config;
            _serviceAddress = serviceAddress;
            _traceMessageHandler = traceMessageHandler;
        }

        public int HitCount { get; }
        public int MissCount { get; }
        public double EfficiencyPercent { get; }

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var analyzerMessages = PerformCallWithRetries(i => i.AnalyzeProject(_config, sourceFilePaths.ToArray(), referencedAssemblyPaths.ToArray()));

            foreach (var analyzerMessage in analyzerMessages)
            {
                switch (analyzerMessage)
                {
                    case IllegalDependencyMessage illegalDependencyMessage:
                        yield return illegalDependencyMessage.IllegalDependency;
                        break;
                    case TraceMessage traceMessage:
                        _traceMessageHandler(traceMessage.Message);
                        break;
                    default:
                        throw new Exception($"Unexpected {nameof(AnalyzerMessageBase)} descendant {analyzerMessage.GetType().Name}");
                }
            }
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            throw new NotImplementedException();
        }

        private TResult PerformCallWithRetries<TResult>(Func<IRemoteDependencyAnalyzer, TResult> serviceOperation)
        {
            Exception lastException = null;
            var keepRetrying = true;
            var retryCount = 0;
            var retryTimeSpans = _config.AnalyzerServiceCallRetryTimeSpans;
            var maxRetryCount = retryTimeSpans.Length;

            while (keepRetrying)
            {
                try
                {
                    var proxy = (IRemoteDependencyAnalyzer)Activator.GetObject(typeof(IRemoteDependencyAnalyzer), _serviceAddress);
                    return serviceOperation.Invoke(proxy);
                }
                catch (Exception e)
                {
                    lastException = e;

                    if (retryCount < maxRetryCount)
                    {
                        var retryTimeSpan = retryTimeSpans[retryCount];
                        _traceMessageHandler($"{CommunicationErrorMessage} Trying to activate service and retrying after {retryTimeSpan}. Exception: {e.Message}");

                        AnalyzerServiceActivator.Activate(_traceMessageHandler);
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
