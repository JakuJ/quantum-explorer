using System;

namespace Compiler
{
    public class OutputReadyArgs : EventArgs
    {
        public OutputReadyArgs(string? diagnostics, string? output)
        {
            Diagnostics = diagnostics;
            Output = output;
        }

        public string? Diagnostics { get; }

        public string? Output { get; }
    }
}
