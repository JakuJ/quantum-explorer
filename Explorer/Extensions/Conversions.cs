using System.Collections.Generic;
using Microsoft.Quantum.Simulation.Core;

namespace Quantum_Server.Extensions
{
    /// <summary>
    /// Extension methods regarding conversions.
    /// </summary>
    public static class Conversions
    {
        /// <summary>
        /// Cast an arbitrary <see cref="IEnumerable{T}"/> to a <see cref="QArray{T}"/> so it can be used in
        /// the quantum simulator context.
        /// </summary>
        /// <param name="collection">An arbitrary enumerable collection of items.</param>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <returns>An instance of <see cref="QArray{T}"/>.</returns>
        public static QArray<T> ToQArray<T>(this IEnumerable<T> collection)
        {
            return new QArray<T>(collection);
        }
    }
}
