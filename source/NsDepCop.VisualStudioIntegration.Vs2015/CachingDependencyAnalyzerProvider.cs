using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves dependency analyzers and caches them for a certain time span.
    /// </summary>
    public class CachingDependencyAnalyzerProvider : TimeBasedCacheBase<string, IDependencyAnalyzer>, IDependencyAnalyzerProvider
    {
        private readonly IDependencyAnalyzerProvider _dependencyAnalyzerProvider;

        public CachingDependencyAnalyzerProvider(IDependencyAnalyzerProvider dependencyAnalyzerProvider,
            IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
            : base(dateTimeProvider, cacheTimeSpan)
        {
            _dependencyAnalyzerProvider = dependencyAnalyzerProvider ?? throw new ArgumentNullException(nameof(dependencyAnalyzerProvider));
        }

        public void Dispose() => _dependencyAnalyzerProvider?.Dispose();

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath) => GetOrAdd(csprojFilePath);

        protected override IDependencyAnalyzer RetrieveItemToCache(string key) => _dependencyAnalyzerProvider.GetDependencyAnalyzer(key);
    }
}
