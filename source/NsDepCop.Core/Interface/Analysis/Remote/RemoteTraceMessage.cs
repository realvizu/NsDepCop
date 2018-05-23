using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// A trace message from a remote dependency analyzer.
    /// </summary>
    [Serializable]
    public class RemoteTraceMessage : IRemoteMessage
    {
        public string Text { get; }

        public RemoteTraceMessage(string text)
        {
            Text = text;
        }
    }
}
