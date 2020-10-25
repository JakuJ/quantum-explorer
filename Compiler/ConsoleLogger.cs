using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler.Diagnostics;
using Diagnostic = Microsoft.VisualStudio.LanguageServer.Protocol.Diagnostic;
using DiagnosticSeverity = Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity;

namespace Compiler
{
    /// <inheritdoc />
    public sealed class ConsoleLogger : LogTracker
    {
        private readonly ILogger<QsCompiler> logger;
        private readonly Func<Diagnostic, string> applyFormatting;

        /// <inheritdoc cref="LogTracker"/>
        /// <summary>Initializes a new instance of the <see cref="ConsoleLogger"/> class.</summary>
        public ConsoleLogger(
            ILogger<QsCompiler> logger,
            Func<Diagnostic, string>? format = null,
            DiagnosticSeverity verbosity = DiagnosticSeverity.Hint,
            IEnumerable<int>? noWarn = null,
            int lineNrOffset = 0)
            : base(verbosity, noWarn, lineNrOffset)
        {
            this.logger = logger;
            applyFormatting = format ?? Formatting.HumanReadableFormat;
        }

        /// <inheritdoc/>
        protected override void Print(Diagnostic msg)
        {
            string message = applyFormatting(msg);
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            logger.LogInformation(msg.Severity.ToString() + " " + message);
        }
    }
}
