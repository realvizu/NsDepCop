using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class ServiceActivator
    {
        public static void ActivateDependencyAnalyzerService()
        {
            var serverName = "NsDepCop.ServiceHost";
            if (!ServerExists(serverName))
            {
                var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase);
                var workingFolder = Path.GetDirectoryName(codeBase.AbsolutePath);
                CreateServer(workingFolder, serverName + ".exe");
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
                Trace.WriteLine($"ServiceActivator.CreateServer failed: {e}");
            }
        }
    }
}
