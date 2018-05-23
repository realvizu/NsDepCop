using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote
{
    /// <summary>
    /// An illegal dependency message from a remote dependency analyzer.
    /// </summary>
    [Serializable]
    public class RemoteIllegalDependencyMessage : IRemoteMessage
    {
        public TypeDependency IllegalDependency { get; }

        public RemoteIllegalDependencyMessage(TypeDependency illegalDependency)
        {
            IllegalDependency = illegalDependency;
        }
    }
}
