using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Extension methods used in other projects.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Waits for the task to complete, unwrapping any exceptions from a possible <see cref="AggregateException"/>.
        /// This method allows to use async functions in non-async scopes.
        /// </summary>
        /// <param name="task">The task. May not be <c>null</c>.</param>
        public static void WaitAndUnwrapException(this Task task) => task.GetAwaiter().GetResult();

        /// <inheritdoc cref="WaitAndUnwrapException"/>
        /// <typeparam name="T">The type of the awaited value.</typeparam>
        /// <returns>The value of the awaited task.</returns>
        public static T WaitAndUnwrapException<T>(this Task<T> task) => task.GetAwaiter().GetResult();

        /// <summary>Enumerate a collection with an index.</summary>
        /// <param name="collection">The collection to be enumerated.</param>
        /// <typeparam name="T">The type of the objects stored in the collection.</typeparam>
        /// <returns>An enumeration over the items of the collection zipped with their indices.</returns>
        public static IEnumerable<(int Index, T Item)> Enumerate<T>(this IEnumerable<T> collection)
        {
            var index = 0;
            foreach (T item in collection)
            {
                yield return (index++, item);
            }
        }

        /// <summary>Truncate a <see cref="List{T}"/> removing elements from the end.</summary>
        /// <param name="list">The list to truncate.</param>
        /// <param name="length">The target list length.</param>
        /// <typeparam name="T">The type of the elements of the list.</typeparam>
        public static void Truncate<T>(this List<T> list, int length) => list.RemoveRange(length, list.Count - length);
    }
}
