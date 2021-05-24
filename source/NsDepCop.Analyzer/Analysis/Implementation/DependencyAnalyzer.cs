using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;
using DotNet.Globbing;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    /// <summary>
    /// Abstract base class for dependency analyzers.
    /// </summary>
    public sealed class DependencyAnalyzer : IDependencyAnalyzer
    {
        private readonly IUpdateableConfigProvider ConfigProvider;
        private readonly MessageHandler TraceMessageHandler;
        private readonly ITypeDependencyEnumerator _typeDependencyEnumerator;
        private readonly object _configRefreshLock = new();

        private CachingTypeDependencyValidator _typeDependencyValidator;
        private IAnalyzerConfig _config;
        private Glob[] _sourcePathExclusionGlobs;

        public DependencyAnalyzer(
            IUpdateableConfigProvider configProvider,
            ITypeDependencyEnumerator typeDependencyEnumerator,
            MessageHandler traceMessageHandler)
        {
            ConfigProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            TraceMessageHandler = traceMessageHandler;
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            UpdateConfig();
        }


        public bool HasConfigError => ConfigProvider.ConfigState == AnalyzerConfigState.ConfigError;
        public bool IsDisabledInConfig => ConfigProvider.ConfigState == AnalyzerConfigState.Disabled;

        public Exception GetConfigException() => ConfigProvider.ConfigException;

        public int MaxIssueCount => ConfigProvider.Config.MaxIssueCount;


        private IEnumerable<AnalyzerMessageBase> AnalyzeCore(Func<IEnumerable<TypeDependency>> illegalTypeDependencyEnumerator)
        {
            return ConfigProvider.ConfigState switch
            {
                AnalyzerConfigState.NoConfig => new NoConfigFileMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Disabled => new ConfigDisabledMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.ConfigError => new ConfigErrorMessage(ConfigProvider.ConfigException).ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Enabled => PerformAnalysis(illegalTypeDependencyEnumerator),
                _ => throw new Exception($"Unexpected ConfigState: {ConfigProvider.ConfigState}")
            };
        }

        private IEnumerable<AnalyzerMessageBase> PerformAnalysis(Func<IEnumerable<TypeDependency>> illegalTypeDependencyEnumerator)
        {
            var config = ConfigProvider.Config;

            foreach (var illegalDependency in illegalTypeDependencyEnumerator())
            {
                yield return new IllegalDependencyMessage(illegalDependency, config.DependencyIssueSeverity);
            }

            // TODO: AutoLowerMaxIssueCount logic should be moved to NsDepCopAnalyzer to act at the end of a compilation.
            // This method is called multiple times during a compilation so we don't know the final issue count here
            //var finalIssueCount = GetInterlocked(ref issueCount);
            //if (config.AutoLowerMaxIssueCount && finalIssueCount < maxIssueCount)
            //    ConfigProvider.UpdateMaxIssueCount(finalIssueCount);
        }

        public IEnumerable<AnalyzerMessageBase> AnalyzeProject(
            IEnumerable<string> sourceFilePaths,
            IEnumerable<string> referencedAssemblyPaths)
        {
            if (sourceFilePaths == null) throw new ArgumentNullException(nameof(sourceFilePaths));
            if (referencedAssemblyPaths == null) throw new ArgumentNullException(nameof(referencedAssemblyPaths));

            if (GlobalSettings.IsToolDisabled())
                return new[] {new ToolDisabledMessage()};

            lock (_configRefreshLock)
            {
                return AnalyzeCore(
                    () => GetIllegalTypeDependencies(
                        () => _typeDependencyEnumerator.GetTypeDependencies(sourceFilePaths, referencedAssemblyPaths, _sourcePathExclusionGlobs))
                );
            }
        }

        public IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            if (syntaxNode == null) throw new ArgumentNullException(nameof(syntaxNode));
            if (semanticModel == null) throw new ArgumentNullException(nameof(semanticModel));

            lock (_configRefreshLock)
            {
                return AnalyzeCore(
                    () => GetIllegalTypeDependencies(
                        () => _typeDependencyEnumerator.GetTypeDependencies(syntaxNode, semanticModel, _sourcePathExclusionGlobs))
                );
            }
        }

        public void RefreshConfig()
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