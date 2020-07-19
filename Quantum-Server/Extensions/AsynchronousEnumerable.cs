using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quantum_Server.Extensions
{
    /// <summary>
    /// A collection of extension methods that introduce LINQ-like transformations
    /// on iterators returning objects that need to be awaited.
    /// </summary>
    public static class AsynchronousEnumerable
    {
        /// <summary>
        /// Like Aggregate, but for async iterators.
        /// </summary>
        /// <param name="source">Source iterator.</param>
        /// <param name="func">A function used to aggregate the results.</param>
        /// <typeparam name="TSource">Type of the source elements.</typeparam>
        /// <returns>A single element of type <typeparamref name="TSource"/> the collection reduces to.</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the sequence is empty.</exception>
        public static async Task<TSource>
            AggregateAsync<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, TSource, Task<TSource>> func)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            TSource result = e.Current;
            while (e.MoveNext())
            {
                result = await func(result, e.Current);
            }

            return result;
        }

        /// <summary>
        /// Like Aggregate, but for async iterators.
        /// </summary>
        /// <param name="source">Source iterator.</param>
        /// <param name="func">A function used to aggregate the results.</param>
        /// <param name="init">The initial value of the accumulator.</param>
        /// <typeparam name="TSource">Type of the source elements.</typeparam>
        /// <typeparam name="TOutput">Type of the result.</typeparam>
        /// <returns>A single element of type <typeparamref name="TSource"/> the collection reduces to.</returns>
        public static async Task<TOutput>
            AggregateAsync<TSource, TOutput>(
                this IEnumerable<TSource> source,
                Func<TOutput, TSource, Task<TOutput>> func,
                TOutput init)
        {
            using IEnumerator<TSource> e = source.GetEnumerator();

            TOutput result = init;
            while (e.MoveNext())
            {
                result = await func(result, e.Current);
            }

            return result;
        }
    }
}
