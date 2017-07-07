using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Implementation.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Service;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;

namespace Codartis.NsDepCop.ServiceHost
{
    /// <summary>
    /// Implements dependency analyzer service as a remoting server.
    /// </summary>
    public class DependencyAnalyzerService : MarshalByRefObject, IDependencyAnalyzerService
    {
        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            var traceMessageBuffer = new List<IEnumerable<string>>();
            var typeDependencyValidator = new CachingTypeDependencyValidator(config);
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(i => traceMessageBuffer.Add(i));

            var typeDependencies = typeDependencyEnumerator.GetTypeDependencies(sourcePaths, referencedAssemblyPaths);
            var illegalDependencies = typeDependencies.Where(i => !typeDependencyValidator.IsAllowedDependency(i)).Take(config.MaxIssueCount);
            traceMessageBuffer.Add(GetCacheStatisticsMessage(typeDependencyValidator).ToList());

            var traceMessages = traceMessageBuffer.Select(i => new AnalyzerMessageBase(new TraceMessage(i)));
            var illegalDependencyMessages = illegalDependencies.Select(i => new AnalyzerMessageBase(new IllegalDependencyMessage(i)));
            return illegalDependencyMessages.ToArray().Concat(traceMessages).ToArray();
        }

        private static IEnumerable<string> GetCacheStatisticsMessage(ICacheStatisticsProvider cache)
        {
            if (cache != null)
                yield return $"Cache hits: {cache.HitCount}, misses: {cache.MissCount}, efficiency (hits/all): {cache.EfficiencyPercent:P}";
        }
    }
}
