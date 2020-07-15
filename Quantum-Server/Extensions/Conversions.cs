using System.Collections.Generic;
using Microsoft.Quantum.Simulation.Core;

namespace Quantum_Server.Extensions
{
    public static class Conversions
    {
        public static QArray<T> ToQArray<T>(this IEnumerable<T> arr)
        {
            return new QArray<T>(arr);
        }
    }
}
