namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands
{
    using System;
    using OneOf;

    public interface ICommand
    {
        Guid CommandId { get; }
        string Name { get; }

        Exception Exception { get; set; }
    }

    public interface ICommand<out TResponse> : ICommand
        where TResponse : class
    {
        TResponse Response { get; }
    }

    [Serializable]
    public class Command<TParameters, TResponse> : ICommand<TResponse>
        where TResponse : class
    {
        public Command(string name, TParameters parameters)
        {
            CommandId = Guid.NewGuid();
            Name = name;
            Parameters = parameters;
            Response = null;
            Exception = null;
        }

        public Guid CommandId { get; }
        public string Name { get; }

        public Exception Exception { get; set; }
        public TParameters Parameters { get; set; }
        public TResponse Response { get; set; }
    }
}