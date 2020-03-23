using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    using System.IO.Pipes;

    /// <summary>
    /// Activates the out-of-process analyzer service.
    /// </summary>
    public static class AnalyzerServiceActivator
    {
        public static RemoteCommandCaller Activate(MessageHandler traceMessageHandler)
        {
            var workingFolder = Assembly.GetExecutingAssembly().GetDirectory();
            var serviceExePath = Path.Combine(workingFolder, ServiceAddressProvider.ServiceHostProcessName + ".exe");

            return CreateServer(workingFolder, serviceExePath, GetProcessId(), traceMessageHandler);
        }

        private static string GetProcessId() => Process.GetCurrentProcess().Id.ToString();

        private static RemoteCommandCaller CreateServer(string workingFolderPath, string serviceExePath, string arguments, MessageHandler traceMessageHandler)
        {
            traceMessageHandler?.Invoke($"Starting {serviceExePath} with parameter {arguments}");
            traceMessageHandler?.Invoke($"  Working folder is {workingFolderPath}");

            var output = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var input = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = serviceExePath,
                    WorkingDirectory = workingFolderPath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Arguments = $"{arguments} {output.GetClientHandleAsString()} {input.GetClientHandleAsString()}",
                };
                Process.Start(processStartInfo);
                 output.DisposeLocalCopyOfClientHandle();
                input.DisposeLocalCopyOfClientHandle();
                return new RemoteCommandCaller(input,  output, traceMessageHandler);
            }
            catch (Exception e)
            {
                traceMessageHandler?.Invoke($"AnalyzerServiceActivator.CreateServer failed: {e}");
                output?.Dispose();
                input?.Dispose();
            }
            return null;
        }
    }
}
