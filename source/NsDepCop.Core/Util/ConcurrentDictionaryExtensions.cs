using System;
using System.Collections.Concurrent;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Extension methods for ConcurrentDictionary.
    /// </summary>
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// Same as the built-in ConcurrentDictionary.GetOrAdd method 
        /// but also tells whether a new value was created or a stored value was returned.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary's value.</typeparam>
        /// <param name="dict">A concurrent dictionary object.</param>
        /// <param name="key">A key.</param>
        /// <param name="generator">A delegate that generates a value for a key.</param>
        /// <param name="added">True if a new value was generated and added, false otherwise.</param>
        /// <returns>The value corresponding for the given key.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict,
            TKey key, Func<TKey, TValue> generator, out bool added)
        {
            TValue value;
            while (true)
            {
                if (dict.TryGetValue(key, out value))
                {
                    added = false;
                    return value;
                }

                value = generator(key);
                if (dict.TryAdd(key, value))
                {
                    added = true;
                    return value;
                }
            }
        }
    }
}
