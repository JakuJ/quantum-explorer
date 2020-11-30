using System.Collections.Generic;
using Common;
using Compiler;

namespace CompilerService
{
    /// <summary>
    /// A payload class used to return values from the compiler Azure Function.
    /// </summary>
    internal class Payload
    {
        public string? Output { get; set; }

        public string? Diagnostics { get; set; }

        public Dictionary<string, GateGrid>? Grids { get; set; }

        public List<OperationState>? States { get; set; }
    }
}
