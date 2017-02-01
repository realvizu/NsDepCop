using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Core.Util
{
    /// <summary>
    /// Static helper class that implements extension methods for a dictionary that has an enumerable value object.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// If the dictionary does not contain the given key then simply adds it with the given values.
        /// If it already contains the key the union's it value with the given values.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's key.</typeparam>
        /// <typeparam name="TEnumerable">The type of the dictionary's value. It must be a collection.</typeparam>
        /// <typeparam name="TValue">The type of the collection's elements.</typeparam>
        /// <param name="dictionary">A dictionary object.</param>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="values">The collection of values to be added or unioned into the dictionary,</param>
        public static void AddOrUnion<TKey, TEnumerable, TValue>(this Dictionary<TKey, TEnumerable> dictionary, TKey key, TEnumerable values)
            where TEnumerable: IEnumerable<TValue>
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = (TEnumerable) dictionary[key].EmptyIfNull().Union(values.EmptyIfNull());
            else
                dictionary.Add(key, values);
        }
    }
}
