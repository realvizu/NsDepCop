﻿using System;
using System.IO;
using Codartis.NsDepCop.Util;

namespace Codartis.NsDepCop.Config.Implementation
{
    /// <summary>
    /// Abstract base class for file based config implementations.
    /// Reloads only if the file changed.
    /// </summary>
    /// <remarks>
    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
    /// </remarks>
    public abstract class FileConfigProviderBase : ConfigProviderBase
    {
        private bool _configFileExists;
        private DateTime _configLastLoadUtc;
        private ConfigLoadResult _lastConfigLoadResult;

        protected string ConfigFilePath { get; }
        protected ConfigFileScope ConfigFileScope { get; }

        protected FileConfigProviderBase(
            string configFilePath, 
            ConfigFileScope configFileScope, 
            MessageHandler traceMessageHandler)
            : base(traceMessageHandler)
        {
            ConfigFilePath = configFilePath;
            ConfigFileScope = configFileScope;
        }

        public override string ConfigLocation => ConfigFilePath;

        public int InheritanceDepth => ConfigBuilder?.InheritanceDepth ?? ConfigDefaults.InheritanceDepth;

        public bool HasConfigFileChanged()
        {
            return ConfigFileCreatedOrDeleted()
                   || ConfigFileModifiedSinceLastLoad();
        }

        protected override ConfigLoadResult LoadConfigCore()
        {
            _lastConfigLoadResult = LoadConfigFromFile();
            return _lastConfigLoadResult;
        }

        protected override ConfigLoadResult RefreshConfigCore()
        {
            if (!HasConfigFileChanged())
                return _lastConfigLoadResult;

            LogTraceMessage($"Refreshing config {this}.");
            return LoadConfigCore();
        }

        private ConfigLoadResult LoadConfigFromFile()
        {
            try
            {
                _configFileExists = File.Exists(ConfigFilePath);
                if (!_configFileExists)
                    return ConfigLoadResult.CreateWithNoConfig();

                _configLastLoadUtc = DateTime.UtcNow;

                var configBuilder = CreateConfigBuilderFromFile(ConfigFilePath)
                    .MakePathsRooted(Path.GetDirectoryName(ConfigFilePath));

                return ConfigLoadResult.CreateWithConfig(configBuilder);
            }
            catch (Exception e)
            {
                LogTraceMessage($"BuildConfig exception: {e}");
                return ConfigLoadResult.CreateWithError(e);
            }
        }

        protected abstract AnalyzerConfigBuilder CreateConfigBuilderFromFile(string configFilePath);

        private bool ConfigFileCreatedOrDeleted()
        {
            return _configFileExists != File.Exists(ConfigFilePath);
        }

        private bool ConfigFileModifiedSinceLastLoad()
        {
            return File.Exists(ConfigFilePath)
                   && _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }

        private void LogTraceMessage(string message) => TraceMessageHandler?.Invoke(message);
    }
}