using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Quantum.Simulation.Core;

namespace Quantum_Server
{
    public static class Conversions
    {
        public static QArray<T> ToQArray<T>(this IEnumerable<T> arr) => new QArray<T>(arr);
    }

    public static class AsynchronousEnumerable
    {
        public static async Task<TSource>
            AggregateAsync<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, TSource, Task<TSource>> func
            )
        {
            using IEnumerator<TSource> e = source.GetEnumerator();

            if (!e.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            TSource result              = e.Current;
            while (e.MoveNext()) result = await func(result, e.Current);
            return result;
        }

        public static async Task<TOutput>
            AggregateAsync<TSource, TOutput>(
                this IEnumerable<TSource> source,
                Func<TOutput, TSource, Task<TOutput>> func,
                TOutput init
            )
        {
            using IEnumerator<TSource> e = source.GetEnumerator();

            TOutput result              = init;
            while (e.MoveNext()) result = await func(result, e.Current);
            return result;
        }
    }
}