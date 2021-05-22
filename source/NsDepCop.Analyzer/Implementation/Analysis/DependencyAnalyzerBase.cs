using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Interface.Analysis.Messages;
using Codartis.NsDepCop.Interface.Config;
using Codartis.NsDepCop.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Implementation.Analysis
{
    /// <summary>
    /// Abstract base class for dependency analyzers.
    /// </summary>
    public abstract class DependencyAnalyzerBase : IDependencyAnalyzer
    {
        protected readonly IUpdateableConfigProvider ConfigProvider;
        protected readonly MessageHandler TraceMessageHandler;

        protected DependencyAnalyzerBase(IUpdateableConfigProvider configProvider, MessageHandler traceMessageHandler)
        {
            ConfigProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            TraceMessageHandler = traceMessageHandler;
        }

        public abstract IEnumerable<AnalyzerMessageBase> AnalyzeProject(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);
        public abstract IEnumerable<AnalyzerMessageBase> AnalyzeSyntaxNode(SyntaxNode syntaxNode, SemanticModel semanticModel, ref int issueCount);
        public abstract void RefreshConfig();

        public bool HasConfigError => ConfigProvider.ConfigState == AnalyzerConfigState.ConfigError;
        public bool IsDisabledInConfig => ConfigProvider.ConfigState == AnalyzerConfigState.Disabled;

        public Exception GetConfigException() => ConfigProvider.ConfigException;

        protected IEnumerable<AnalyzerMessageBase> AnalyzeCore(Func<IEnumerable<TypeDependency>> illegalTypeDependencyEnumerator, ref int issueCount)
        {
            return ConfigProvider.ConfigState switch
            {
                AnalyzerConfigState.NoConfig => new NoConfigFileMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Disabled => new ConfigDisabledMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.ConfigError => new ConfigErrorMessage(ConfigProvider.ConfigException).ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Enabled => PerformAnalysis(illegalTypeDependencyEnumerator, ref issueCount),
                _ => throw new Exception($"Unexpected ConfigState: {ConfigProvider.ConfigState}")
            };
        }

        private IEnumerable<AnalyzerMessageBase> PerformAnalysis(Func<IEnumerable<TypeDependency>> illegalTypeDependencyEnumerator, ref int issueCount)
        {
            var config = ConfigProvider.Config;
            var maxIssueCount = config.MaxIssueCount;

            if (GetInterlocked(ref issueCount) > maxIssueCount)
                return Enumerable.Empty<AnalyzerMessageBase>();

            var messages = new List<AnalyzerMessageBase>();

            foreach (var illegalDependency in illegalTypeDependencyEnumerator())
            {
                var currentIssueCount = Interlocked.Increment(ref issueCount);

                if (currentIssueCount > maxIssueCount)
                {
                    messages.Add(new TooManyIssuesMessage(maxIssueCount, config.MaxIssueCountSeverity));
                    break;
                }

                messages.Add(new IllegalDependencyMessage(illegalDependency, config.DependencyIssueSeverity));
            }

            // TODO: AutoLowerMaxIssueCount logic should be moved to NsDepCopAnalyzer to act at the end of a compilation.
            // This method is called multiple times during a compilation so we don't know the final issue count here
            //var finalIssueCount = GetInterlocked(ref issueCount);
            //if (config.AutoLowerMaxIssueCount && finalIssueCount < maxIssueCount)
            //    ConfigProvider.UpdateMaxIssueCount(finalIssueCount);

            return messages;
        }

        private static int GetInterlocked(ref int issueCount) => Interlocked.CompareExchange(ref issueCount, 0, 0);
    }
}