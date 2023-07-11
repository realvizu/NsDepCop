using System;

namespace Codartis.NsDepCop.Analysis.Implementation
{
    public class DependencyStatus
    {
        private DependencyStatus()
        {
        }

        public bool IsAllowed { get; private set; }

        public string[] AllowedTypeNames { get; private set; } = Array.Empty<string>();

        public static readonly DependencyStatus Allowed = new() { IsAllowed = true };

        public static readonly DependencyStatus Disallowed = new() { IsAllowed = false };

        public static DependencyStatus DisallowedWithinSpecifiedConstraints(string[] allowedTypeNames) =>
            new() { IsAllowed = false, AllowedTypeNames = allowedTypeNames };
    }
}