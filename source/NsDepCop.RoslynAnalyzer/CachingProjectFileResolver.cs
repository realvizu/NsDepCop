using System;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Resolves project files from assembly names and caches them for a certain time span.
    /// </summary>
    public class CachingProjectFileResolver : TimeBasedCacheBase<string, string>, IProjectFileResolver
    {
        private readonly IProjectFileResolver _projectFileResolver;

        public CachingProjectFileResolver(IProjectFileResolver projectFileResolver,
            IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
            : base(dateTimeProvider, cacheTimeSpan)
        {
            _projectFileResolver = projectFileResolver ?? throw new ArgumentNullException(nameof(projectFileResolver));
        }

        public string FindByAssemblyName(string assemblyName) => GetOrAdd(assemblyName);

        protected override string RetrieveItemToCache(string key) => _projectFileResolver.FindByAssemblyName(key);
    }
}
