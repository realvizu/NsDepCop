namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using System.Threading.Tasks;
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
    using Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands;
    using Codartis.NsDepCop.Core.Util;

    public class RemoteCommandCaller : IDisposable
    {
        private const int Timeout = 30;
        private readonly ServerStreams RemoteServer;
        private readonly MessageHandler TraceMessageHandler;
        private readonly Dictionary<Guid, Tuple<DateTime, object>> openTasks = new Dictionary<Guid, Tuple<DateTime, object>>();
        private bool disposed = false;

        public RemoteCommandCaller(ServerStreams remoteServer, MessageHandler traceMessageHandler)
        {
            this.RemoteServer = remoteServer;
            TraceMessageHandler = traceMessageHandler;
            Task.Factory.StartNew(CleanupTask);
            Task.Factory.StartNew(ReceiveTask);
        }

        public bool Connected => RemoteServer.Input != null && RemoteServer.Output != null;

        public Task<TResult> Call<TParameter, TResult>(Command<TParameter, TResult> command)
            where TParameter : class
            where TResult : class
        {
            var task = new TaskCompletionSource<TResult>();
            var serializer = new BinaryFormatter();
            lock (openTasks)
            {
                openTasks.Add(command.CommandId, Tuple.Create(DateTime.Now + TimeSpan.FromSeconds(Timeout), (object)task));
                serializer.Serialize(RemoteServer.Input, command);
            }

            return task.Task;
        }

        private void CleanupTask()
        {
            while (!disposed)
            {
                KeyValuePair<Guid, Tuple<DateTime, object>>[] toDelete;
                lock (openTasks)
                {
                    toDelete = openTasks.Where(e => e.Value.Item1 < DateTime.Now).ToArray();
                    foreach (var entry in toDelete)
                    {
                        openTasks.Remove(entry.Key);
                    }
                }

                foreach (var entry in toDelete)
                {
                    SendException(entry.Value.Item2, new TimeoutException());
                }

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        private void ReceiveTask()
        {
            while (!disposed)
            {
                try
                {
                    ReadNextResponse();
                }
                catch (Exception e)
                {
                    TraceMessageHandler?.Invoke(e.ToString());
                }
            }
        }

        private void ReadNextResponse()
        {
            var serializer = new BinaryFormatter();
            var response = serializer.Deserialize(RemoteServer.Output);
            if (response is ICommand<object> command)
            {
                Tuple<DateTime, object> value;
                lock (openTasks)
                {
                    if (!openTasks.TryGetValue(command.CommandId, out value))
                    {
                        return;
                    }

                    openTasks.Remove(command.CommandId);
                }

                SendResponseOrException(value.Item2, command);
            }
        }

        private void SendResponseOrException(object tcs, ICommand<object> command)
        {
            if (command.Exception != null)
            {
                SendException(tcs, command.Exception);
            }
            else
            {
                try
                {
                    SendResponse(tcs, command.Response);
                }
                catch (Exception e)
                {
                    SendException(tcs, e);
                }
            }
        }

        private  void SendResponse(object tcs, object response)
        {
            tcs.GetType().InvokeMember(
                nameof(TaskCompletionSource<object>.SetResult),
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                null,
                tcs,
                new object [] {response});
        }

        private  void SendException(object tcs, Exception e)
        {
            try
            {
                tcs.GetType().InvokeMember(
                    nameof(TaskCompletionSource<object>.SetException),
                    BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                    null,
                    tcs,
                    new object [] {e});
            }
            catch (Exception exception)
            {
                TraceMessageHandler?.Invoke(exception.ToString());
            }
        }

        public void Dispose()
        {
            disposed = true;
            RemoteServer.Input?.Dispose();
            RemoteServer.Output?.Dispose();
        }
    }
}