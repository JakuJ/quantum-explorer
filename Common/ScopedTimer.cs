using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Common
{
    /// <inheritdoc />
    /// <summary>
    /// A class that measures time from object creation to its disposal.
    /// </summary>
    /// <example><code>
    /// using (new ScopedTimer( ...action or message... ))
    /// {
    ///     ... some code that takes a long time to execute ...
    /// }
    /// </code></example>
    public class ScopedTimer : IDisposable
    {
        private readonly Stopwatch stopwatch;
        private readonly Action<Stopwatch>? action;
        private readonly ILogger logger;

        /// <inheritdoc cref="ScopedTimer(ILoggerFactory)"/>
        /// <param name="message">The message to be printed after instance disposal.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public ScopedTimer(string message, ILoggerFactory loggerFactory) : this(loggerFactory)
        {
            action = watch => { logger.LogInformation($"{message} took: {watch.ElapsedMilliseconds}ms"); };
        }

        /// <inheritdoc cref="ScopedTimer(ILoggerFactory)"/>
        /// <param name="action">An <see cref="Action"/> to perform after disposal.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public ScopedTimer(Action action, ILoggerFactory loggerFactory) : this(loggerFactory) => this.action = _ => { action(); };

        /// <inheritdoc cref="ScopedTimer(Action, ILoggerFactory)"/>
        public ScopedTimer(Action<Stopwatch> action, ILoggerFactory loggerFactory) : this(loggerFactory) => this.action = action;

        /// <summary>Initializes a new instance of the <see cref="ScopedTimer"/> class.</summary>
        /// <param name="loggerFactory">Logger factory.</param>
        private ScopedTimer(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<ScopedTimer>();
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
