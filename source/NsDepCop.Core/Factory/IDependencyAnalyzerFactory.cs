using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Factory
{
    /// <summary>
    /// Creates dependency analyzer objects.
    /// </summary>
    public interface IDependencyAnalyzerFactory
    {
        /// <summary>
        /// Creates a dependency analyzer object for an xml config file.
        /// </summary>
        /// <param name="configFilePath">The full path of the xml config file.</param>
        /// <param name="overridingParser">Parser that overrides the one specified in the config file. Optional.</param>
        /// <param name="diagnosticMessageHandler">Callback for emitting internal diagnostic messages. Optional.</param>
        /// <returns>A new dependency analyzer instance.</returns>
        IDependencyAnalyzer CreateFromXmlConfigFile(string configFilePath, Parsers? overridingParser = null,
            Action<string> diagnosticMessageHandler = null);
    }
}