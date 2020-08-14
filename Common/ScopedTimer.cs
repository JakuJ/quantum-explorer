using System;
using System.Diagnostics;

// TODO: Inject the logging service when available, instead of Console.WriteLine'ing.
namespace Common
{
    /// <inheritdoc />
    /// <summary>
    /// A class that measures time from object creation to its disposal.
    /// </summary>
    public class ScopedTimer : IDisposable
    {
        private readonly Stopwatch stopwatch;
        private readonly string action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedTimer"/> class.
        /// </summary>
        /// <param name="action">A name to be placed in the message.</param>
        public ScopedTimer(string action)
        {
            this.action = action;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            stopwatch.Stop();
            Console.Error.WriteLineAsync($"{action} took: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
