using System;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Describes a version redirect from an old version interval to a new version.
    /// </summary>
    internal struct VersionRedirect
    {
        public Version OldVersionFrom { get; }
        public Version OldVersionTo { get; }
        public Version NewVersion { get; }

        public VersionRedirect(Version oldVersionFrom, Version oldVersionTo, Version newVersion)
        {
            OldVersionFrom = oldVersionFrom ?? throw new ArgumentNullException(nameof(oldVersionFrom));
            OldVersionTo = oldVersionTo ?? throw new ArgumentNullException(nameof(oldVersionTo));
            NewVersion = newVersion ?? throw new ArgumentNullException(nameof(newVersion));
        }

        public bool Match(Version version) => OldVersionFrom <= version && version <= OldVersionTo;

        public override string ToString() => $"{OldVersionFrom}-{OldVersionTo} -> {NewVersion}";
    }
}