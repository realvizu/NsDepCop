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
        private readonly CachingTypeDependencyValidator _typeDependencyValidator;

        public DependencyAnalyzer(IAnalyzerConfig config, ITypeDependencyEnumerator typeDependencyEnumerator, MessageHandler traceMessageHandler)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            _typeDependencyValidator = new CachingTypeDependencyValidator(_config, traceMessageHandler);
        }

        public int HitCount => _typeDependencyValidator?.HitCount ?? 0;
        public int MissCount => _typeDependencyValidator?.MissCount ?? 0;
        public double EfficiencyPercent => _typeDependencyValidator?.EfficiencyPercent ?? 0;

        public IEnumerable<TypeDependency> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            var typeDependencies = _typeDependencyEnumerator.GetTypeDependencies(sourceFilePaths, referencedAssemblyPaths);
            return GetIllegalDependencies(typeDependencies);
        }

        public IEnumerable<TypeDependency> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            var typeDependencies = _typeDependencyEnumerator.GetTypeDependencies(syntaxNode, semanticModel);
            return GetIllegalDependencies(typeDependencies);
        }

        private IEnumerable<TypeDependency> GetIllegalDependencies(IEnumerable<TypeDependency> typeDependencies)
        {
            return typeDependencies
                .Where(i => !_typeDependencyValidator.IsAllowedDependency(i))
                .Take(_config.MaxIssueCount);
        }
    }
}
