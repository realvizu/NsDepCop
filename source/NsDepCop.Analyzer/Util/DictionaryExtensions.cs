using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Util
{
    /// <summary>
    /// Static helper class that implements extension methods for a dictionary whose value is a collection type.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// If the dictionary does not contain the given key then simply adds it with the given values.
        /// If it already contains the key the union's it value with the given values.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary's key.</typeparam>
        /// <typeparam name="TCollection">The type of the dictionary's value. It must be a collection.</typeparam>
        /// <typeparam name="TValue">The type of the collection's elements.</typeparam>
        /// <param name="dictionary">A dictionary object.</param>
        /// <param name="key">The key to be added or updated.</param>
        /// <param name="newValues">The collection of newValues to be added or unioned into the dictionary,</param>
        public static void AddOrUnion<TKey, TCollection, TValue>(this Dictionary<TKey, TCollection> dictionary, TKey key, TCollection newValues)
            where TCollection : class, ICollection<TValue>, new()
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, newValues);
                return;
            }

            var oldValues = dictionary[key];
            if (oldValues == null && newValues != null)
            {
                dictionary[key] = newValues;
                return;
            }

            if (oldValues != null && newValues != null)
            {
                var newCollection = new TCollection();
                foreach (var value in oldValues.Union(newValues))
                    newCollection.Add(value);
                dictionary[key] = newCollection;
            }
        }
    }
}
