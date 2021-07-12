using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Quantum.QsCompiler.Diagnostics;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using static Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity;

namespace Compiler
{
    /// <inheritdoc cref="LogTracker"/>
    public sealed class EventLogger : LogTracker
    {
        private readonly Action<string> logAction;
        private readonly Func<Diagnostic, string> applyFormatting;

        /// <inheritdoc cref="LogTracker"/>
        /// <summary>Initializes a new instance of the <see cref="EventLogger"/> class.</summary>
        public EventLogger(
            Action<string> logAction,
            Func<Diagnostic, string>? format = null,
            DiagnosticSeverity verbosity = Hint,
            IEnumerable<int>? noWarn = null,
            int lineNrOffset = 0)
            : base(verbosity, noWarn, lineNrOffset)
        {
            this.logAction = logAction;
            applyFormatting = format ?? Formatting.HumanReadableFormat;
        }

        /// <summary>Gets a value indicating whether any errors were logged during compilation.</summary>
        public bool SeenErrors { get; private set; }

        /// <inheritdoc/>
        protected override void Print(Diagnostic msg)
        {
            string message = applyFormatting(msg);

            // QS7202 is not important to end user and leaks server-side file paths
            if (msg is { Severity: Error } or { Severity: Warning, Code: not "QS7202" })
            {
                var withFilesSkipped = string.Join('\n', message.Split('\n').Where(x => !x.StartsWith("File:")));
                logAction(withFilesSkipped);
            }

            if (msg is { Severity: Error })
            {
                SeenErrors = true;
            }
        }
    }
}
