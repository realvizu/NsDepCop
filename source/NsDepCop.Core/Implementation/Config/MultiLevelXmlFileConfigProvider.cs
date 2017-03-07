//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Codartis.NsDepCop.Core.Interface;
//using Codartis.NsDepCop.Core.Interface.Config;
//using Codartis.NsDepCop.Core.Util;

//namespace Codartis.NsDepCop.Core.Implementation.Config
//{
//    /// <summary>
//    /// Traverses the source tree and reads all config files to create a composite config.
//    /// Starts from the specified folder and traverses the folder tree upwards till the root is reached.
//    /// </summary>
//    /// <remarks>
//    /// Base class ensures that all operations are executed in an atomic way so no extra locking needed.
//    /// </remarks>
//    internal class MultiLevelXmlFileConfigProvider : ConfigProviderBase
//    {
//        /// <summary>
//        /// Just a precaution to avoid runaway folder traversals.
//        /// </summary>
//        private const int MaxFolderLevelsToTraverse = 10;

//        private readonly string _projectFolder;
//        private Dictionary<string, XmlFileConfigProvider> _fileConfigProvidersByConfigPath;

//        public MultiLevelXmlFileConfigProvider(string projectFolder, Parsers? overridingParser = null, Action<string> diagnosticMessageHandler = null)
//            :base(overridingParser, diagnosticMessageHandler)
//        {
//            _projectFolder = projectFolder;
//            _fileConfigProvidersByConfigPath = new Dictionary<string, XmlFileConfigProvider>();
//        }

//        public override string ToString() => $"MultiLevelXmlConfig:'{_projectFolder}'";

//        protected override AnalyzerState GetState()
//        {
//            if (!_fileConfigProviders.Any())
//                return AnalyzerState.NoConfigFile;

//            if (_fileConfigProviders.Any(i => i.ConfigException != null))
//                return AnalyzerState.ConfigError;

//            if (IsConfigLoaded && !Config.IsEnabled)
//                return AnalyzerState.Disabled;

//            if (IsConfigLoaded && Config.IsEnabled)
//                return AnalyzerState.Enabled;

//            throw new Exception("Inconsistent DependencyAnalyzer state.");
//        }

//        protected override AnalyzerConfigBuilder GetConfigBuilder()
//        {
//            _fileConfigProviders = CreateConfigProviders();

//            var configBuilder = new AnalyzerConfigBuilder(OverridingParser);

//            foreach (var configProvider in Enumerable.Reverse(_fileConfigProviders))
//            {
//                DiagnosticMessageHandler?.Invoke($"Found config file: '{configProvider.ConfigFilePath}'");

//                if (configProvider.State == AnalyzerState.Enabled)
//                    configBuilder.Combine(configProvider.ConfigBuilder);
//            }

//            return configBuilder;
//        }

//        private List<XmlFileConfigProvider> CreateConfigProviders()
//        {
//            return FileFinder.FindInParentFolders(ProductConstants.DefaultConfigFileName, _projectFolder, MaxFolderLevelsToTraverse)
//                .Select(configFilePath => new XmlFileConfigProvider(configFilePath, OverridingParser, DiagnosticMessageHandler))
//                .ToList();
//        }
//    }
//}
