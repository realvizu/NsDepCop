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
        private readonly IUpdateableConfigProvider _configProvider;
        private readonly MessageHandler _traceMessageHandler;
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
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _traceMessageHandler = traceMessageHandler;
            _typeDependencyEnumerator = typeDependencyEnumerator ?? throw new ArgumentNullException(nameof(typeDependencyEnumerator));
            UpdateConfig();
        }


        public AnalyzerConfigState ConfigState
        {
            get
            {
                lock (_configRefreshLock)
                {
                    return _configProvider.ConfigState;
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (_configRefreshLock)
                {
                    return _configProvider.ConfigException;
                }
            }
        }

        public IAnalyzerConfig Config
        {
            get
            {
                lock (_configRefreshLock)
                {
                    return _configProvider.Config;
                }
            }
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
                _configProvider.RefreshConfig();
                UpdateConfig();
            }
        }

        private void UpdateConfig()
        {
            var oldConfig = _config;
            _config = _configProvider.Config;

            if (oldConfig == _config)
                return;

            _typeDependencyValidator = CreateTypeDependencyValidator();
            _sourcePathExclusionGlobs = _config.SourcePathExclusionPatterns.Select(Glob.Parse).ToArray();
        }

        private CachingTypeDependencyValidator CreateTypeDependencyValidator()
        {
            return _configProvider.ConfigState == AnalyzerConfigState.Enabled
                ? new CachingTypeDependencyValidator(_configProvider.Config, _traceMessageHandler)
                : null;
        }

        private IEnumerable<AnalyzerMessageBase> AnalyzeCore(Func<IEnumerable<IllegalTypeDependency>> illegalTypeDependencyEnumerator)
        {
            return _configProvider.ConfigState switch
            {
                AnalyzerConfigState.NoConfig => new NoConfigFileMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Disabled => new ConfigDisabledMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.ConfigError => new ConfigErrorMessage(_configProvider.ConfigException).ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Enabled => PerformAnalysis(illegalTypeDependencyEnumerator),
                _ => throw new Exception($"Unexpected ConfigState: {_configProvider.ConfigState}")
            };
        }

        private static IEnumerable<AnalyzerMessageBase> PerformAnalysis(Func<IEnumerable<IllegalTypeDependency>> illegalTypeDependencyEnumerator)
        {
            return illegalTypeDependencyEnumerator().Select(i => new IllegalDependencyMessage(i.TypeDependency, i.AllowedMembers));

            // TODO: AutoLowerMaxIssueCount logic should be moved to NsDepCopAnalyzer to act at the end of a compilation.
            // This method is called multiple times during a compilation so we don't know the final issue count here
            //var finalIssueCount = GetInterlocked(ref issueCount);
            //if (config.AutoLowerMaxIssueCount && finalIssueCount < maxIssueCount)
            //    ConfigProvider.UpdateMaxIssueCount(finalIssueCount);
        }

        private IEnumerable<IllegalTypeDependency> GetIllegalTypeDependencies(Func<IEnumerable<TypeDependency>> typeDependencyEnumerator)
        {
            var allDeps = typeDependencyEnumerator().Select(dep => (dep, _typeDependencyValidator.IsAllowedDependency(dep)));
            
            var excessIllegalDeps = allDeps
                .Where(i => !i.Item2.IsAllowed)
                .Take(_config.MaxIssueCount + 1);
            
            // var illegalDependencies = typeDependencyEnumerator()
            //     .Where(i => !_typeDependencyValidator.IsAllowedDependency(i).IsAllowed)
            //     .Take(_config.MaxIssueCount + 1);

            foreach (var illegalDependency in excessIllegalDeps)
            {
                if (illegalDependency.Item2.AllowedTypeNames != Array.Empty<string>())
                {
                    yield return new IllegalTypeDependency(illegalDependency.dep, illegalDependency.Item2.AllowedTypeNames);
                    
                }
                yield return new IllegalTypeDependency(illegalDependency.dep, Array.Empty<string>());
            }

            _traceMessageHandler?.Invoke(GetCacheStatisticsMessage(_typeDependencyValidator));
        }

        private static string GetCacheStatisticsMessage(ICacheStatisticsProvider i) =>
            $"Cache hits: {i.HitCount}, misses: {i.MissCount}, efficiency (hits/all): {i.EfficiencyPercent:P}";
    }

    public class IllegalTypeDependency
    {
        public TypeDependency TypeDependency { get; }
        
        public string[] AllowedMembers { get; }
    
        public IllegalTypeDependency(TypeDependency typeDependency, string[] allowedMembers)
        {
            TypeDependency = typeDependency;
            AllowedMembers = allowedMembers;
        }
    }
}