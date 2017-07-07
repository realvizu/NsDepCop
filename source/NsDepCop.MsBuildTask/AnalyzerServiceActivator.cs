using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Activates the out-of-process analyzer service.
    /// </summary>
    public static class AnalyzerServiceActivator
    {
        private const string ServiceHostProcessName = "NsDepCop.ServiceHost";

        public static void Activate()
        {
            if (!ServerExists(ServiceHostProcessName))
            {
                var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var workingFolder = Path.GetDirectoryName(codeBase.AbsolutePath);
                CreateServer(workingFolder, ServiceHostProcessName + ".exe");
            }
        }

        private static bool ServerExists(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private static void CreateServer(string workingFolderPath, string serviceExeName)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(workingFolderPath, serviceExeName),
                    WorkingDirectory = workingFolderPath,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    Arguments = Process.GetCurrentProcess().Id.ToString(),
                };
                Process.Start(processStartInfo);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"AnalyzerServiceActivator.CreateServer failed: {e}");
            }
        }
    }
}
