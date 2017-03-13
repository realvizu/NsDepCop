using System;
using System.Collections.Concurrent;
using Codartis.NsDepCop.Core.Interface.Analysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves dependency analyzers and caches them for a certain time span.
    /// </summary>
    internal class CachingDependencyAnalyzerProvider : IDependencyAnalyzerProvider
    {
        /// <summary>
        /// Dependency analyzer retrieval operations are delegated to this object.
        /// </summary>
        private readonly DependencyAnalyzerProvider _dependencyAnalyzerProvider;

        /// <summary>
        /// The max lifetime of a cached item.
        /// </summary>
        private readonly TimeSpan _cachingTimeSpan;

        /// <summary>
        /// Cache that maps csproj file path to a cache item which contains a dependency analyzer and the time when it was retrieved.
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheItem> _cache;

        public CachingDependencyAnalyzerProvider(TimeSpan cacheTimeSpan, Action<string> diagnosticMessageHandler = null)
        {
            _dependencyAnalyzerProvider = new DependencyAnalyzerProvider(diagnosticMessageHandler);
            _cachingTimeSpan = cacheTimeSpan;
            _cache = new ConcurrentDictionary<string, CacheItem>();
        }

        public void Dispose()
        {
            _dependencyAnalyzerProvider?.Dispose();
        }

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath)
        {
            var cacheItem = _cache.GetOrAdd(csprojFilePath, CreateCacheItem);

            if (DateTime.UtcNow <= cacheItem.RetrievalDateTime + _cachingTimeSpan)
                return cacheItem.DependencyAnalyzer;

            var newCacheItem = CreateCacheItem(csprojFilePath);
            _cache.TryUpdate(csprojFilePath, newCacheItem, cacheItem);
            return newCacheItem.DependencyAnalyzer;
        }

        private CacheItem CreateCacheItem(string csprojFilePath)
        {
            var dependencyAnalyzer = _dependencyAnalyzerProvider.GetDependencyAnalyzer(csprojFilePath);
            return new CacheItem(dependencyAnalyzer, DateTime.UtcNow);
        }

        /// <summary>
        /// Bundles together a dependency analyzer and the date time when it was retrieved.
        /// </summary>
        private struct CacheItem
        {
            public IDependencyAnalyzer DependencyAnalyzer { get; }
            public DateTime RetrievalDateTime { get; }

            public CacheItem(IDependencyAnalyzer dependencyAnalyzer, DateTime retrievalDateTime)
            {
                DependencyAnalyzer = dependencyAnalyzer;
                RetrievalDateTime = retrievalDateTime;
            }
        }
    }
}
