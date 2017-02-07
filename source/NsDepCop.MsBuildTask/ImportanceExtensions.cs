using Codartis.NsDepCop.Core.Interface.Config;
using Microsoft.Build.Framework;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class ImportanceExtensions
    {
        public static MessageImportance ToMessageImportance(this Importance importance)
        {
            switch (importance)
            {
                case Importance.Low:
                    return MessageImportance.Low;
                case Importance.High:
                    return MessageImportance.High;
                default:
                    return MessageImportance.Normal;
            }
        }
    }
}