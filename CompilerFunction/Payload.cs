using System.Collections.Generic;
using Common;
using Compiler;

namespace CompilerService
{
    public class Payload
    {
        public string? Output { get; set; }

        public string? Diagnostics { get; set; }

        public Dictionary<string, GateGrid> Grids { get; set; }

        public List<OperationState>? States { get; set; }
    }
}
