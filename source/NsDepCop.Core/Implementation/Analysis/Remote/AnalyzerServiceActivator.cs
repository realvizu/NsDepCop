using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    using System.Collections.Generic;
    using System.IO.Pipes;

    /// <summary>
    /// Activates the out-of-process analyzer service.
    /// </summary>
    public static class AnalyzerServiceActivator
    {
        public static ServerStreams Activate(MessageHandler traceMessageHandler)
        {
            var workingFolder = Assembly.GetExecutingAssembly().GetDirectory();
            var serviceExePath = Path.Combine(workingFolder, ServiceAddressProvider.ServiceHostProcessName + ".exe");

            return CreateServer(workingFolder, serviceExePath, GetProcessId(), traceMessageHandler);
        }

        private static string GetProcessId() => Process.GetCurrentProcess().Id.ToString();

        private static ServerStreams CreateServer(string workingFolderPath, string serviceExePath, string arguments, MessageHandler traceMessageHandler)
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
                return new ServerStreams(input,  output);
            }
            catch (Exception e)
            {
                traceMessageHandler?.Invoke($"AnalyzerServiceActivator.CreateServer failed: {e}");
                 output?.Dispose();
                input?.Dispose();
            }
            return new ServerStreams(null, null);
        }
    }

    public struct ServerStreams
    {
        public Stream Input;
        public Stream Output;

        public ServerStreams(Stream input, Stream output)
        {
            Input = input;
            Output = output;
        }

        public override bool Equals(object obj)
        {
            return obj is ServerStreams other &&
                   EqualityComparer<Stream>.Default.Equals(Input, other.Input) &&
                   EqualityComparer<Stream>.Default.Equals(Output, other.Output);
        }

        public override int GetHashCode()
        {
            var hashCode = -1030903623;
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(Input);
            hashCode = hashCode * -1521134295 + EqualityComparer<Stream>.Default.GetHashCode(Output);
            return hashCode;
        }

        public void Deconstruct(out Stream item1, out Stream item2)
        {
            item1 = Input;
            item2 = Output;
        }

        /* implicit conversions to ValueTuple which is supported beginning from Framework version 4.7
        public static implicit operator (Stream, Stream)(ServerStreams value)
        {
            return (value.Input, value.Output);
        }

        public static implicit operator ServerStreams((Stream, Stream) value)
        {
            return new ServerStreams(value.Item1, value.Item2);
        }
        */
    }
}
