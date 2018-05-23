using System;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Messages
{
    /// <summary>
    /// A message indicating that analysis was disabled in the config file.
    /// </summary>
    [Serializable]
    public class ConfigDisabledMessage : InfoMessageBase
    {
        public ConfigDisabledMessage() 
            : base (InfoMessageType.ConfigDisabled)
        {
        }

        public override string ToString() => IssueDefinitions.ConfigDisabledIssue.StaticDescription;
    }
}
