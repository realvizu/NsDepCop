using System;
using System.Collections.Concurrent;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves dependency analyzers and caches them for a certain time span.
    /// </summary>
    public class CachingDependencyAnalyzerProvider : IDependencyAnalyzerProvider
    {
        private readonly IDependencyAnalyzerProvider _dependencyAnalyzerProvider;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _cachingTimeSpan;

        /// <summary>
        /// Cache that maps csproj file path to a cache item which contains a dependency analyzer and the time when it was retrieved.
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheItem> _cache;

        public CachingDependencyAnalyzerProvider(IDependencyAnalyzerProvider dependencyAnalyzerProvider, IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
        {
            if (dependencyAnalyzerProvider == null)
                throw new ArgumentNullException(nameof(dependencyAnalyzerProvider));

            if (dateTimeProvider == null)
                throw new ArgumentNullException(nameof(dateTimeProvider));

            _dependencyAnalyzerProvider = dependencyAnalyzerProvider;
            _dateTimeProvider = dateTimeProvider;
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

            if (_dateTimeProvider.UtcNow < cacheItem.RetrievalDateTime + _cachingTimeSpan)
                return cacheItem.DependencyAnalyzer;

            var newCacheItem = CreateCacheItem(csprojFilePath);
            _cache.TryUpdate(csprojFilePath, newCacheItem, cacheItem);
            return newCacheItem.DependencyAnalyzer;
        }

        private CacheItem CreateCacheItem(string csprojFilePath)
        {
            var dependencyAnalyzer = _dependencyAnalyzerProvider.GetDependencyAnalyzer(csprojFilePath);
            return new CacheItem(dependencyAnalyzer, _dateTimeProvider.UtcNow);
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
