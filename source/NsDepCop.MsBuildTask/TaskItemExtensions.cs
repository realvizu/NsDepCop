using Microsoft.Build.Framework;

namespace Codartis.NsDepCop.MsBuildTask
{
    public static class TaskItemExtensions
    {
        public static string GetValue(this ITaskItem taskItem) => taskItem?.ItemSpec;
    }
}