using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Analysis.Messages;
using Codartis.NsDepCop.Config;
using Codartis.NsDepCop.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    public sealed class AssemblyDependencyAnalyzer : IAssemblyDependencyAnalyzer
    {
        private readonly IUpdateableConfigProvider _configProvider;
        private readonly MessageHandler _traceMessageHandler;
        private readonly object _configRefreshLock = new();

        private IAssemblyDependencyValidator _assemblyDependencyValidator;
        private IAnalyzerConfig _config;

        public AssemblyDependencyAnalyzer(
            IUpdateableConfigProvider configProvider,
            MessageHandler traceMessageHandler)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _traceMessageHandler = traceMessageHandler;
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
            AssemblyIdentity sourceAssembly,
            IReadOnlyList<AssemblyIdentity> referencedAssemblies)
        {
            if (sourceAssembly == null) throw new ArgumentNullException(nameof(sourceAssembly));
            if (referencedAssemblies == null) throw new ArgumentNullException(nameof(referencedAssemblies));

            if (GlobalSettings.IsToolDisabled())
                return [new ToolDisabledMessage()];

            lock (_configRefreshLock)
            {
                var assemblyDependencyEnumerable = GetAssemblyDependencies(sourceAssembly, referencedAssemblies);
                var illegalAssemblyDependencyEnumerable = GetIllegalAssemblyDependencies(assemblyDependencyEnumerable);
                return AnalyzeCore(illegalAssemblyDependencyEnumerable);
            }
        }

        private IEnumerable<AssemblyDependency> GetAssemblyDependencies(AssemblyIdentity sourceAssembly, IReadOnlyList<AssemblyIdentity> referencedAssemblies)
        {
            foreach (AssemblyIdentity referencedAssembly in referencedAssemblies)
            {
                yield return new AssemblyDependency(sourceAssembly, referencedAssembly);
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

            _assemblyDependencyValidator = CreateTypeDependencyValidator();
        }

        private AssemblyDependencyValidator CreateTypeDependencyValidator()
        {
            return _configProvider.ConfigState == AnalyzerConfigState.Enabled
                ? new AssemblyDependencyValidator(_configProvider.Config)
                : null;
        }

        private IEnumerable<AnalyzerMessageBase> AnalyzeCore(IEnumerable<AssemblyDependency> illegalTypeDependencyEnumerable)
        {
            return _configProvider.ConfigState switch
            {
                AnalyzerConfigState.NoConfig => new NoConfigFileMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Disabled => new ConfigDisabledMessage().ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.ConfigError => new ConfigErrorMessage(_configProvider.ConfigException).ToEnumerable<AnalyzerMessageBase>(),
                AnalyzerConfigState.Enabled => PerformAnalysis(illegalTypeDependencyEnumerable),
                _ => throw new Exception($"Unexpected ConfigState: {_configProvider.ConfigState}")
            };
        }

        private static IEnumerable<AnalyzerMessageBase> PerformAnalysis(IEnumerable<AssemblyDependency> illegalTypeDependencyEnumerator)
        {
            return illegalTypeDependencyEnumerator.Select(element => new IllegalAssemblyDependencyMessage(element));
        }

        private IEnumerable<AssemblyDependency> GetIllegalAssemblyDependencies(IEnumerable<AssemblyDependency> assemblyDependencyEnumerator)
        {
            foreach (AssemblyDependency assemblyDependency in assemblyDependencyEnumerator)
            {
                DependencyStatus dependencyStatus = _assemblyDependencyValidator.IsDependencyAllowed(assemblyDependency);
                if (!dependencyStatus.IsAllowed)
                {
                    yield return assemblyDependency;
                }
            }
        }
    }
}