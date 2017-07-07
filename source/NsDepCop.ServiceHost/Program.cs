using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Threading;
using Codartis.NsDepCop.Core.Interface;
using Codartis.NsDepCop.Core.Interface.Analysis.Service;

namespace Codartis.NsDepCop.ServiceHost
{
    /// <summary>
    /// Host process for the dependency analyzer remoting service.
    /// </summary>
    /// <remarks>
    /// Monitors the parent process and quits if the parent no longer exists.
    /// </remarks>
    public class Program
    {
        private static readonly string PipeName = $"{ProductConstants.ToolName}-{ProductConstants.Version}";
        private static readonly string ServiceName = $"{nameof(IDependencyAnalyzerService)}-{ProductConstants.Version}";

        /// <summary>
        /// Entry point of the application.
        /// </summary>
        /// <param name="args">The first parameter is the ID of the parent process.</param>
        /// <returns>Zero: normal exit. Negative value: error.</returns>
        public static int Main(string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var parentProcessId))
            {
                Usage();
                return -1;
            }

            try
            {
                RegisterRemotingService();

                while (IsProcessAlive(parentProcessId))
                    Thread.Sleep(1000);

                return 0;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[{ProductConstants.ToolName}] ServiceHost exception caught: {e}");
                Console.WriteLine($"Exception caught: {e}");
                return -2;
            }
        }

        private static void RegisterRemotingService()
        {
            ChannelServices.RegisterChannel(new IpcChannel(PipeName), false);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(DependencyAnalyzerService),
                ServiceName, 
                WellKnownObjectMode.SingleCall);
        }

        private static bool IsProcessAlive(int processId)
        {
            try
            {
                return !Process.GetProcessById(processId).HasExited;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void Usage()
        {
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} <parentprocessid>");
        }
    }
}