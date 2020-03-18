using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands;

    /// <summary>
    /// Client that performs dependency analysis by calling a service via remoting.
    /// </summary>
    /// <remarks>
    /// Syntax node analysis and config refreshing are not supported.
    /// </remarks>
    public sealed class RemoteDependencyAnalyzerClient : DependencyAnalyzerBase
    {
        private const string CommunicationErrorMessage = "Unable to communicate with NsDepCop service.";

        private readonly string _serviceAddress;
        private RemoteCommandCaller remoteCommandCaller;

        public RemoteDependencyAnalyzerClient(IUpdateableConfigProvider configProvider, string serviceAddress, MessageHandler traceMessageHandler)
            : base(configProvider, traceMessageHandler)
        {
            _serviceAddress = serviceAddress ?? throw new ArgumentNullException(nameof(serviceAddress));
        }

        public override IEnumerable<AnalyzerMessageBase> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            return GlobalSettings.IsToolDisabled()
                ? new[] { new ToolDisabledMessage() }
                : AnalyzeCore(() => GetIllegalTypeDependencies(sourceFilePaths, referencedAssemblyPaths), isProjectScope: true);
        }

        public override IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            throw new NotSupportedException();
        }

        public override void RefreshConfig()
        {
            throw new NotSupportedException();
        }

        private IEnumerable<TypeDependency> GetIllegalTypeDependencies(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var retryTimeSpans = ConfigProvider.Config.AnalyzerServiceCallRetryTimeSpans;
            var retryCount = 0;

            var retryResult = RetryHelper.Retry(
                () => InvokeRemoteAnalyzer(sourceFilePaths, referencedAssemblyPaths),
                retryTimeSpans.Length,
                e => ActivateServerAndWaitBeforeRetry(e, retryCount++, retryTimeSpans));

            return retryResult.Match(
                UnwrapTraceMessages,
                OnAllRetriesFailed);
        }

        private IRemoteMessage[] InvokeRemoteAnalyzer(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            TraceMessageHandler?.Invoke("Calling analyzer service.");

            if (remoteCommandCaller == null || !remoteCommandCaller.Connected)
            {
                throw new InvalidOperationException("The remote analyzer service is not yet started");
            }

            var command = new AnalyzeProjectCommand(new AnalyzeProjectCommand.ParameterType()
            {
                Config = ConfigProvider.Config,
                ReferencedAssemblyPaths = referencedAssemblyPaths.ToArray(),
                SourcePaths = sourceFilePaths.ToArray()

            });

            var result = remoteCommandCaller.Call(command).Result;

            TraceMessageHandler?.Invoke("Calling analyzer service succeeded.");

            return result;
        }

        private void ActivateServerAndWaitBeforeRetry(Exception e, int retryCount, TimeSpan[] retryTimeSpans)
        {
            TraceMessageHandler?.Invoke($"{CommunicationErrorMessage} Exception: {e.Message}");

            TraceMessageHandler?.Invoke($"Trying to activate analyzer service (attempt #{retryCount + 1}).");
            if (remoteCommandCaller == null || !remoteCommandCaller.Connected)
            {
                remoteCommandCaller?.Dispose();
                var serverStreams = AnalyzerServiceActivator.Activate(TraceMessageHandler);
                remoteCommandCaller = new RemoteCommandCaller(serverStreams, TraceMessageHandler);

                var sleepTimeSpan = retryTimeSpans[retryCount];
                TraceMessageHandler?.Invoke($"Retrying service call after: {sleepTimeSpan}.");
                Thread.Sleep(sleepTimeSpan);
            }
        }

        private IEnumerable<TypeDependency> UnwrapTraceMessages(IRemoteMessage[] remoteMessages)
        {
            foreach (var remoteMessage in remoteMessages)
            {
                switch (remoteMessage)
                {
                    case RemoteIllegalDependencyMessage illegalDependencyMessage:
                        yield return illegalDependencyMessage.IllegalDependency;
                        break;
                    case RemoteTraceMessage traceMessage:
                        TraceMessageHandler?.Invoke(traceMessage.Text);
                        break;
                    default:
                        throw new Exception($"Unexpected IRemoteMessage type {remoteMessage?.GetType().Name}");
                }
            }
        }

        private static IEnumerable<TypeDependency> OnAllRetriesFailed(Exception exception)
        {
            throw new Exception($"{CommunicationErrorMessage} All retries failed.", exception);
        }
    }
}