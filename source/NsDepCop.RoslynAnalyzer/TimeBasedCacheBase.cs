using System;
using System.Collections.Concurrent;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Abstract base class for caches that cache for a certain time span.
    /// </summary>
    /// <typeparam name="TKey">The type of the key in the cache.</typeparam>
    /// <typeparam name="TCachedItem">The type of the cached items.</typeparam>
    public abstract class TimeBasedCacheBase<TKey, TCachedItem> 
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly TimeSpan _cachingTimeSpan;
        private readonly ConcurrentDictionary<TKey, CacheEntry> _cache;

        protected TimeBasedCacheBase(IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _cachingTimeSpan = cacheTimeSpan;
            _cache = new ConcurrentDictionary<TKey, CacheEntry>();
        }

        protected TCachedItem GetOrAdd(TKey key)
        {
            var cacheItem = _cache.GetOrAdd(key, CreateCacheItem);

            if (_dateTimeProvider.UtcNow < cacheItem.CachedAt + _cachingTimeSpan)
                return cacheItem.CachedItem;

            var newCacheItem = CreateCacheItem(key);
            _cache.TryUpdate(key, newCacheItem, cacheItem);
            return newCacheItem.CachedItem;
        }

        protected abstract TCachedItem RetrieveItemToCache(TKey key);

        private CacheEntry CreateCacheItem(TKey key)
        {
            return new CacheEntry(RetrieveItemToCache(key), _dateTimeProvider.UtcNow);
        }

        /// <summary>
        /// Bundles together a cached item and the date time when it was cached.
        /// </summary>
        private struct CacheEntry
        {
            public TCachedItem CachedItem { get; }
            public DateTime CachedAt { get; }

            public CacheEntry(TCachedItem cachedItem, DateTime cachedAt)
            {
                CachedItem = cachedItem;
                CachedAt = cachedAt;
            }
        }
    }
}
