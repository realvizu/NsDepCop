using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    /// <summary>
    /// Activates the out-of-process analyzer service.
    /// </summary>
    public static class AnalyzerServiceActivator
    {
        public static void Activate(MessageHandler traceMessageHandler)
        {
            var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            var workingFolder = Path.GetDirectoryName(codeBase.AbsolutePath);
            if (workingFolder == null)
                throw new Exception($"Unable to determine working folder from assembly codebase: {codeBase.AbsolutePath}");

            var serviceExePath = Path.Combine(workingFolder, ServiceAddressProvider.ServiceHostProcessName + ".exe");

            CreateServer(workingFolder, serviceExePath, GetProcessId(), traceMessageHandler);
        }

        private static string GetProcessId() => Process.GetCurrentProcess().Id.ToString();

        private static void CreateServer(string workingFolderPath, string serviceExePath, string arguments, MessageHandler traceMessageHandler)
        {
            traceMessageHandler($"Starting {serviceExePath} with parameter {arguments}");
            traceMessageHandler($"  Working folder is {workingFolderPath}");

            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = serviceExePath,
                    WorkingDirectory = workingFolderPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = arguments,
                };
                Process.Start(processStartInfo);
            }
            catch (Exception e)
            {
                traceMessageHandler($"AnalyzerServiceActivator.CreateServer failed: {e}");
            }
        }
    }
}
