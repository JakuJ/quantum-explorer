using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Quantum.QsCompiler.Diagnostics;
using Diagnostic = Microsoft.VisualStudio.LanguageServer.Protocol.Diagnostic;
using DiagnosticSeverity = Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity;

namespace Compiler
{
    /// <inheritdoc />
    public sealed class EventLogger : LogTracker
    {
        private static readonly HashSet<string> Ignored = new HashSet<string>(new[]
        {
            "QS7202", // not important to end user, leaks server-side file paths
        });

        private readonly Action<string> logAction;
        private readonly Func<Diagnostic, string> applyFormatting;

        /// <inheritdoc cref="LogTracker"/>
        /// <summary>Initializes a new instance of the <see cref="EventLogger"/> class.</summary>
        public EventLogger(
            Action<string> logAction,
            Func<Diagnostic, string>? format = null,
            DiagnosticSeverity verbosity = DiagnosticSeverity.Hint,
            IEnumerable<int>? noWarn = null,
            int lineNrOffset = 0)
            : base(verbosity, noWarn, lineNrOffset)
        {
            this.logAction = logAction;
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

            if ((msg.Severity == DiagnosticSeverity.Error
              || msg.Severity == DiagnosticSeverity.Warning)
             && !Ignored.Contains(msg.Code))
            {
                var withFilesSkipped = string.Join('\n', message.Split('\n').Where(x => !x.StartsWith("File:")));
                logAction(withFilesSkipped);
            }
        }
    }
}
