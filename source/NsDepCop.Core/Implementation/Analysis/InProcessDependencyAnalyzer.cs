using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;
using DotNet.Globbing;

namespace Codartis.NsDepCop.Core.Implementation.Analysis
{
    /// <summary>
    /// Performs in-process dependency analysis.
    /// </summary>
    internal class InProcessDependencyAnalyzer : DependencyAnalyzerBase
    {
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly object _configRefreshLock = new object();

        private CachingTypeDependencyValidator _typeDependencyValidator;
        private IAnalyzerConfig _config;
        private Glob[] _sourcePathExclusionGlobs;

        public InProcessDependencyAnalyzer(
            IUpdateableConfigProvider configProvider,
            ITypeDependencyEnumerator typeDependencyEnumerator,
            MessageHandler traceMessageHandler)
            : base(configProvider, traceMessageHandler)
        {
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            UpdateConfig();
        }

        public override IEnumerable<AnalyzerMessageBase> AnalyzeProject(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            if (sourceFilePaths == null) throw new ArgumentNullException(nameof(sourceFilePaths));
            if (referencedAssemblyPaths == null) throw new ArgumentNullException(nameof(referencedAssemblyPaths));

            if (GlobalSettings.IsToolDisabled())
                return new[] { new ToolDisabledMessage() };

            lock (_configRefreshLock)
            {
                return AnalyzeCore(
                    () => GetIllegalTypeDependencies(
                        () => _typeDependencyEnumerator.GetTypeDependencies(
                            sourceFilePaths,
                            referencedAssemblyPaths,
                            _sourcePathExclusionGlobs)),
                    isProjectScope: true);
            }
        }

        public override IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(ISyntaxNode syntaxNode, ISemanticModel semanticModel)
        {
            if (syntaxNode == null) throw new ArgumentNullException(nameof(syntaxNode));
            if (semanticModel == null) throw new ArgumentNullException(nameof(semanticModel));

            if (GlobalSettings.IsToolDisabled())
                return new[] { new ToolDisabledMessage() };

            lock (_configRefreshLock)
            {
                return AnalyzeCore(
                    () => GetIllegalTypeDependencies(
                        () => _typeDependencyEnumerator.GetTypeDependencies(
                            syntaxNode,
                            semanticModel,
                            _sourcePathExclusionGlobs)),
                    isProjectScope: false);
            }
        }

        public override void RefreshConfig()
        {
            lock (_configRefreshLock)
            {
                ConfigProvider.RefreshConfig();
                UpdateConfig();
            }
        }

        private void UpdateConfig()
        {
            var oldConfig = _config;
            _config = ConfigProvider.Config;

            if (oldConfig == _config)
                return;

            _typeDependencyValidator = CreateTypeDependencyValidator();
            _sourcePathExclusionGlobs = _config.SourcePathExclusionPatterns.Select(Glob.Parse).ToArray();
        }

        private CachingTypeDependencyValidator CreateTypeDependencyValidator()
        {
            return ConfigProvider.ConfigState == AnalyzerConfigState.Enabled
                ? new CachingTypeDependencyValidator(ConfigProvider.Config, TraceMessageHandler)
                : null;
        }

        private IEnumerable<TypeDependency> GetIllegalTypeDependencies(Func<IEnumerable<TypeDependency>> typeDependencyEnumerator)
        {
            var illegalDependencies = typeDependencyEnumerator()
                .Where(i => !_typeDependencyValidator.IsAllowedDependency(i))
                .Take(_config.MaxIssueCount + 1);

            foreach (var illegalDependency in illegalDependencies)
                yield return illegalDependency;

            TraceMessageHandler?.Invoke(GetCacheStatisticsMessage(_typeDependencyValidator));
        }

        private static string GetCacheStatisticsMessage(ICacheStatisticsProvider i) =>
            $"Cache hits: {i.HitCount}, misses: {i.MissCount}, efficiency (hits/all): {i.EfficiencyPercent:P}";
    }
}