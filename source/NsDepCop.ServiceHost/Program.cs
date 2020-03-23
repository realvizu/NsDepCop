using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface;

namespace Codartis.NsDepCop.ServiceHost
{
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands;
    using System.IO.Pipes;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote;

    /// <summary>
    /// Host process for the dependency analyzer remoting service.
    /// </summary>
    /// <remarks>
    /// Monitors the parent process and quits if the parent no longer exists.
    /// </remarks>
    public class Program
    {
        private static bool stop = false;

        /// <summary>
        /// Entry point of the application.
        /// </summary>
        /// <param name="args">The first parameter is the ID of the parent process.</param>
        /// <returns>Zero: normal exit. Negative value: error.</returns>
        public static int Main(string[] args)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out var parentProcessId))
            {
                Usage();
                return -1;
            }

            var outputStreamHandle = args[1];
            var inputStreamHandle = args[2];


            try
            {
                var input = new AnonymousPipeClientStream(PipeDirection.In, inputStreamHandle);
                var output = new AnonymousPipeClientStream(PipeDirection.Out, outputStreamHandle);

                Task.Run(() => ServeWhileNotStopped(input, output));

                WaitForParentProcessExit(parentProcessId);
                return 0;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"[{ProductConstants.ToolName}] ServiceHost exception caught: {e}");
                Console.WriteLine($"Exception caught: {e}");
                return -2;
            }
        }

        private static void ServeWhileNotStopped(AnonymousPipeClientStream input, AnonymousPipeClientStream output)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            while (!stop)
            {
                object inputObject = serializer.Deserialize(input);
                Task.Run(() => ServeOne(inputObject, output));
            }
        }

        private static void ServeOne(object inputObject, AnonymousPipeClientStream output)
        {
            if (inputObject is AnalyzeProjectCommand analyzeCommand)
            {
                var result = TryToRun(analyzeCommand, RunAnalyzeProject);
                analyzeCommand.Parameters = null;
                analyzeCommand.Response = result;
                SendResult(output, analyzeCommand);
            }
            else if (inputObject is ICommand command)
            {
                command.Exception = new InvalidOperationException($"This command is unknown {command.Name}");
                SendResult(output, command);
            }
        }

        private static void SendResult(AnonymousPipeClientStream output, object command)
        {
            lock (output)
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(output, command);
            }
        }

        private static TResult TryToRun<T, TResult>(T command, Func<T, TResult> func)
            where TResult : class
            where T : ICommand<TResult>
        {
            try
            {
                var result = func(command);
                command.Exception = null;
                return result;
            }
            catch (Exception e)
            {
                command.Exception = e;
            }
            return null;
        }

        private static IRemoteMessage[] RunAnalyzeProject(AnalyzeProjectCommand analyzeCommand)
        {
            var server = new RemoteDependencyAnalyzerServer();
            var result = server.AnalyzeProject(
                analyzeCommand.Parameters.Config,
                analyzeCommand.Parameters.SourcePaths,
                analyzeCommand.Parameters.ReferencedAssemblyPaths);
            return result;
        }

        private static void WaitForParentProcessExit(int parentProcessId)
        {
            var parentProcess = Process.GetProcesses().FirstOrDefault(i => i.Id == parentProcessId);
            parentProcess?.WaitForExit();
            stop = true;
        }

        private static void Usage()
        {
            Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName().Name} <parentprocessid> <outputHandle> <inputHandle>");
        }
    }
}