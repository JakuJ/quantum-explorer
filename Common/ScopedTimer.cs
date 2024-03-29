using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common
{
    /// <inheritdoc cref="IDisposable"/>
    /// <summary>
    /// A class that measures time from object creation to its disposal.
    /// </summary>
    /// <example>
    ///     <code>
    /// using (new ScopedTimer( ...action or message... ))
    /// {
    ///     ... some code that takes a long time to execute ...
    /// }
    ///     </code>
    /// </example>
    public sealed class ScopedTimer : IDisposable
    {
        private readonly Stopwatch stopwatch;
        private readonly Action<Stopwatch>? action;

        /// <inheritdoc cref="ScopedTimer()"/>
        /// <param name="message">The message to be printed after instance disposal.</param>
        /// <param name="logger">A <see cref="Logger"/> instance to log the message with.</param>
        public ScopedTimer(string message, ILogger logger) : this()
        {
            action = watch => { logger.LogDebug($"{message} took {watch.ElapsedMilliseconds}ms"); };
        }

        /// <inheritdoc cref="ScopedTimer()"/>
        /// <param name="action">An <see cref="Action"/> to perform after disposal.</param>
        public ScopedTimer(Action action) : this() => this.action = _ => { action(); };

        /// <inheritdoc cref="ScopedTimer(Action)"/>
        public ScopedTimer(Action<Stopwatch> action) : this() => this.action = action;

        /// <summary>Initializes a new instance of the <see cref="ScopedTimer"/> class.</summary>
        private ScopedTimer()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            stopwatch.Stop();
            action?.Invoke(stopwatch);
        }
    }
}
