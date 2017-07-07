using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// A dependency analyzer with a fixed config.
    /// </summary>
    internal class DependencyAnalyzer : IDependencyAnalyzer
    {
        private readonly IAnalyzerConfig _config;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly MessageHandler _traceMessageHandler;
        private readonly CachingTypeDependencyValidator _typeDependencyValidator;

        public DependencyAnalyzer(IAnalyzerConfig config,
            ITypeDependencyEnumerator typeDependencyEnumerator,
            MessageHandler traceMessageHandler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _traceMessageHandler = traceMessageHandler;
            _typeDependencyValidator = new CachingTypeDependencyValidator(_config, _traceMessageHandler);
        }

        public int HitCount => _typeDependencyValidator?.HitCount ?? 0;
        public int MissCount => _typeDependencyValidator?.MissCount ?? 0;
        public double EfficiencyPercent => _typeDependencyValidator?.EfficiencyPercent ?? 0;

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var illegalDependencies = GetIllegalDependencies(_typeDependencyEnumerator.GetTypeDependencies(sourceFilePaths, referencedAssemblyPaths));
            _traceMessageHandler?.Invoke(GetCacheStatisticsMessage());
            return illegalDependencies;
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            return GetIllegalDependencies(_typeDependencyEnumerator.GetTypeDependencies(syntaxNode, semanticModel));
        }

        private IEnumerable<TypeDependency> GetIllegalDependencies(IEnumerable<TypeDependency> typeDependencies)
        {
            return typeDependencies.Where(i => !_typeDependencyValidator.IsAllowedDependency(i)).Take(_config.MaxIssueCount);
        }

        private string GetCacheStatisticsMessage() =>
            $"Cache hits: {HitCount}, misses: {MissCount}, efficiency (hits/all): {EfficiencyPercent:P}";
    }
}
