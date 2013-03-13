using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Codartis.NsCop.Core
{
    /// <summary>
    /// Static helper class that implements extension methods for IEnumerable[T]
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Returns an empty collection for a null value.
        /// </summary>
        /// <typeparam name="T">Any type.</typeparam>
        /// <param name="collection">A collection.</param>
        /// <returns>The collection itself if it was not null or an empty collection if it was null.</returns>
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection ?? Enumerable.Empty<T>();
        }

        /// <summary>
        /// Converts a collection into a single string by concatenating the string representation of the collection elements 
        /// using optional separator and wrapper strings.
        /// </summary>
        /// <typeparam name="T">Any type.</typeparam>
        /// <param name="collection">A collection.</param>
        /// <param name="separator">A string that will be put between every to item. Null means no separator.</param>
        /// <param name="leftWrapper">A string that will precede every item. Null means no left wrapper.</param>
        /// <param name="rightWrapper">A string that will follow every item. Null means no right wrapper.</param>
        /// <returns>The string represenation of the collection.</returns>
        public static string ToSingleString<T>(this IEnumerable<T> collection, string separator, string leftWrapper, string rightWrapper)
        {
            var stringBuilder = new StringBuilder();

            foreach (var item in collection)
            {
                // Apply separator if necessary.
                if (stringBuilder.Length > 0)
                {
                    if (!string.IsNullOrEmpty(separator))
                        stringBuilder.Append(separator);
                }

                // Apply left wrapper string if necessary.
                if (!string.IsNullOrEmpty(leftWrapper))
                    stringBuilder.Append(leftWrapper);

                // Append the item's string representation.
                stringBuilder.Append(item.ToString());

                // Apply right wrapper string if necessary.
                if (!string.IsNullOrEmpty(rightWrapper))
                    stringBuilder.Append(rightWrapper);
            }

            return stringBuilder.ToString();
        }
    }
}
