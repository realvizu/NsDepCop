using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that no config file was found for a project or location.
    /// </summary>
    [Serializable]
    public class NoConfigFileMessage : InfoMessageBase
    {
        public NoConfigFileMessage()
            : base(InfoMessageType.NoConfigFile)
        {
        }

        public override string ToString() => IssueDefinitions.NoConfigFileIssue.StaticDescription;
    }
}