using System;
using System.Collections.Generic;
using System.Diagnostics;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Abstract base class for dependency analyzer implementations.
    /// </summary>
    public abstract class DependencyAnalyzerBase : IDependencyAnalyzer
    {
        protected readonly IRuleConfig Config;
        protected readonly ITypeDependencyValidator TypeDependencyValidator;

        protected DependencyAnalyzerBase(IRuleConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Config = config;
            TypeDependencyValidator = new CachingTypeDependencyValidator(config);
        }

        public IEnumerable<DependencyViolation> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            foreach (var dependencyViolation in AnalyzeProjectOverride(sourceFilePaths, referencedAssemblyPaths))
                yield return dependencyViolation;

            DebugDumpCacheStatistics(TypeDependencyValidator);
        }

        public IEnumerable<DependencyViolation> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            foreach (var dependencyViolation in AnalyzeSyntaxNodeOverride(syntaxNode, semanticModel))
                yield return dependencyViolation;

            DebugDumpCacheStatistics(TypeDependencyValidator);
        }

        protected abstract IEnumerable<DependencyViolation> AnalyzeProjectOverride(
            IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);

        protected abstract IEnumerable<DependencyViolation> AnalyzeSyntaxNodeOverride(
            ISyntaxNode syntaxNode, ISemanticModel semanticModel);

        private static void DebugDumpCacheStatistics(object o)
        {
            var cache = o as ICacheStatisticsProvider;
            if (cache == null)
                return;

            Debug.WriteLine($"Cache hits: {cache.HitCount}, misses:{cache.MissCount}, efficiency (hits/all): {cache.EfficiencyPercent:P}",
                ProductConstants.ToolName);
        }
    }
}