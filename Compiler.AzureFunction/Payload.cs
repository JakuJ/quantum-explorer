using System.Collections.Generic;
using Common;

namespace Compiler.AzureFunction
{
    /// <summary>
    /// A payload class used to return values from the compiler Azure Function.
    /// </summary>
    public class Payload
    {
        /// <summary>Gets or sets the output from the execution.</summary>
        public string? Output { get; set; }

        /// <summary>Gets or sets compilation diagnostics diagnostics.</summary>
        public string? Diagnostics { get; set; }

        /// <summary>Gets or sets GateGrids extracted from the compilation.</summary>
        public Dictionary<string, GateGrid>? Grids { get; set; }

        /// <summary>Gets or sets quantum states intercepted during the simulation.</summary>
        public List<OperationState>? States { get; set; }
    }
}
