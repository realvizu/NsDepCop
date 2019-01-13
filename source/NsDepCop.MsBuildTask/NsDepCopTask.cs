using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis.Messages;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Implements a custom MsBuild task that performs namespace dependency analysis and reports disallowed dependencies.
    /// </summary>
    /// <remarks>
    /// Invokes the analyzer from an out-of-process server via remoting 
    /// to avoid DLL version conflicts with the host process and to improve performance.
    /// </remarks>
    public class NsDepCopTask : Task
    {
        /// <summary>
        /// MsBuild task item list that contains the name and full path 
        /// of the assemblies referenced in the current project.
        /// </summary>
        [Required]
        public ITaskItem[] ReferencePath { get; set; }

        /// <summary>
        /// MsBuild task item list that contains the name and relative path
        /// of the source files in the current project.
        /// The paths are relative to the BaseDirectory.
        /// </summary>
        [Required]
        public ITaskItem[] Compile { get; set; }

        /// <summary>
        /// MsBuild task item that contains the full path of the directory of the project file.
        /// </summary>
        [Required]
        public ITaskItem BaseDirectory { get; set; }

        /// <summary>
        /// Not used any more.
        /// </summary>
        public ITaskItem Parser { get; set; }

        /// <summary>
        /// Specifies the info log events' message importance level. Optional. Default: Normal.
        /// </summary>
        public ITaskItem InfoImportance { get; set; }

        private ILogger _logger;

        /// <summary>
        /// Parameterless ctor is needed by MsBuild.
        /// </summary>
        public NsDepCopTask()
        {
            // Remoting loads assemblies at deserialization (even already loaded ones) 
            // and we must help it to find the NsDepCop assemblies.
            DirectoryBasedAssemblyResolver.Initialize(Assembly.GetExecutingAssembly().GetDirectory());
        }

        /// <summary>
        /// This ctor is for unit testing. 
        /// </summary>
        /// <param name="logger">A logger object.</param>
        public NsDepCopTask(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string ProjectFolder => BaseDirectory.ItemSpec;
        private IEnumerable<string> SourceFilePaths => Compile.Select(i => i.ItemSpec.ToAbsolutePath(ProjectFolder));
        private IEnumerable<string> ReferencedAssemblyPaths => ReferencePath.Select(i => i.ItemSpec);

        /// <summary>
        /// Executes the custom MsBuild task. Called by the MsBuild tool.
        /// </summary>
        /// <returns>
        /// True if there was no error and no exception.
        /// </returns>
        public override bool Execute()
        {
            if (_logger == null)
            {
                // This must not be moved to the ctor because BuildEngine is not yet initialized at construction time.
                _logger = new MsBuildLoggerGateway(BuildEngine);
            }

            try
            {
                if (IsToolDisabled())
                {
                    _logger.LogIssue(new ToolDisabledMessage());
                    return true;
                }

                var runWasSuccessful = true;

                _logger.LogTraceMessage(GetInputParameterDiagnosticMessages());

                var defaultInfoImportance = EnumHelper.ParseNullable<Importance>(InfoImportance.GetValue());
                var analyzerFactory = new DependencyAnalyzerFactory(_logger.LogTraceMessage).SetDefaultInfoImportance(defaultInfoImportance);
                var analyzer = analyzerFactory.CreateOutOfProcess(ProjectFolder, ServiceAddressProvider.ServiceAddress);
                var analyzerMessages = analyzer.AnalyzeProject(SourceFilePaths, ReferencedAssemblyPaths);

                _logger.InfoImportance = analyzer.InfoImportance.ToMessageImportance();

                foreach (var analyzerMessage in analyzerMessages)
                {
                    switch (analyzerMessage)
                    {
                        case InfoMessageBase infoMessage:
                            _logger.LogInfo(infoMessage);
                            break;

                        case IssueMessageBase issueMessage:
                            _logger.LogIssue(issueMessage);
                            runWasSuccessful = runWasSuccessful && issueMessage.IssueKind != IssueKind.Error;
                            break;

                        default:
                            throw new Exception($"Unexpected analyzer message type: {analyzerMessage?.GetType().Name}");
                    }
                }

                return runWasSuccessful;
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception during NsDepCopTask execution: {e}");
                return false;
            }
        }

        private static bool IsToolDisabled()
        {
            var environmentVariableValue = Environment.GetEnvironmentVariable(ProductConstants.DisableToolEnvironmentVariableName);

            return environmentVariableValue == "1" ||
                   string.Equals(environmentVariableValue, "true", StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<string> GetInputParameterDiagnosticMessages()
        {
            yield return $"{ProductConstants.ToolName} started with parameters:";

            yield return $"  ReferencePath[{ReferencePath.Length}]";
            foreach (var taskItem in ReferencePath)
                yield return $"    {taskItem.ItemSpec}";

            yield return $"  Compile[{Compile.Length}]";
            foreach (var taskItem in Compile)
                yield return $"    {taskItem.ItemSpec}";

            yield return $"  BaseDirectory={BaseDirectory.ItemSpec}";
        }
    }
}