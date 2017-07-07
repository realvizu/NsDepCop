using System;
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
            var resultBuilder = new AnalyzeProjectResultBuilder();

            var typeDependencyValidator = new CachingTypeDependencyValidator(config, i => resultBuilder.AddTrace(i));
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(i => resultBuilder.AddTrace(i));

            var typeDependencies = typeDependencyEnumerator.GetTypeDependencies(sourcePaths, referencedAssemblyPaths);
            var illegalDependencies = typeDependencies.Where(i => !typeDependencyValidator.IsAllowedDependency(i)).Take(config.MaxIssueCount);

            foreach (var illegalDependency in illegalDependencies)
                resultBuilder.AddIllegalDependency(illegalDependency);

            resultBuilder.AddTrace(GetCacheStatisticsMessage(typeDependencyValidator));

            return resultBuilder.ToArray();
        }

        private static string GetCacheStatisticsMessage(ICacheStatisticsProvider cache) => 
            $"Cache hits: {cache.HitCount}, misses: {cache.MissCount}, efficiency (hits/all): {cache.EfficiencyPercent:P}";
    }
}
