using System.Collections.Generic;

namespace Compiler
{
    /// <summary>
    /// A simple class representing a grid of quantum gates.
    /// Meant to be used by the compositor.
    /// </summary>
    public class GateGrid
    {
        private readonly List<string> gates = new List<string>();

        /// <summary>
        /// Add a gate to the grid.
        /// </summary>
        /// <param name="gate">The name of the gate.</param>
        public void AddGate(string gate) => gates.Add(gate);
    }
}
