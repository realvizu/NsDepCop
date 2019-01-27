using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using DotNet.Globbing;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    /// <summary>
    /// Abstract base class for a dependency analyzer service implemented as a remoting server. Stateless.
    /// </summary>
    /// <remarks>
    /// This class is abstract because the Core package cannot reference a concrete TypeDependencyEnumerator,
    /// but it cannot get it as a ctor parameter either 
    /// because a server activated remoting server can have only parameterless ctor
    /// so it must acquire the TypeDependencyEnumerator using a template method.
    /// </remarks>
    public abstract class RemoteDependencyAnalyzerServerBase : MarshalByRefObject, IRemoteDependencyAnalyzer
    {
        public IRemoteMessage[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            var messageBuffer = new List<IRemoteMessage>();

            var typeDependencyEnumerator = GetTypeDependencyEnumerator(WrapIntoTraceMessage(messageBuffer));
            var typeDependencyValidator = new CachingTypeDependencyValidator(config, WrapIntoTraceMessage(messageBuffer));
            var sourcePathExclusionGlobs = config.SourcePathExclusionPatterns.Select(Glob.Parse);

            var illegalDependencyMessages = typeDependencyEnumerator
                .GetTypeDependencies(sourcePaths, referencedAssemblyPaths, sourcePathExclusionGlobs)
                .Where(i => !typeDependencyValidator.IsAllowedDependency(i))
                .Take(config.MaxIssueCount + 1)
                .Select(i => new RemoteIllegalDependencyMessage(i));

            messageBuffer.AddRange(illegalDependencyMessages);

            WrapIntoTraceMessage(messageBuffer)(GetCacheStatisticsMessage(typeDependencyValidator));

            return messageBuffer.ToArray();
        }

        protected abstract ITypeDependencyEnumerator GetTypeDependencyEnumerator(MessageHandler traceMessageHandler);

        private static MessageHandler WrapIntoTraceMessage(List<IRemoteMessage> messageBuffer)
        {
            return i => messageBuffer.Add(new RemoteTraceMessage(i));
        }

        private static string GetCacheStatisticsMessage(ICacheStatisticsProvider i) =>
            $"Cache hits: {i.HitCount}, misses: {i.MissCount}, efficiency (hits/all): {i.EfficiencyPercent:P}";
    }
}