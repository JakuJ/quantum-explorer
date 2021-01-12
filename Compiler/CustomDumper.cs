using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using static Microsoft.Quantum.Simulation.Simulators.QuantumSimulator;

namespace Compiler
{
    /// <inheritdoc cref="StateDumper"/>
    public class CustomDumper : StateDumper
    {
        /// <inheritdoc cref="StateDumper"/>
        /// <summary>Initializes a new instance of the <see cref="CustomDumper"/> class.</summary>
        /// <param name="simulator">Simulator that is being dumped.</param>
        public CustomDumper(QuantumSimulator simulator) : base(simulator) { }

        /// <summary>
        /// Gets last dumped quantum states.
        /// </summary>
        public List<(int Idx, Complex Value)> Values { get; private set; } = new();

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage] // reason: used by external APIs only
        public override bool Callback(uint idx, double real, double img)
        {
            Values.Add(((int)idx, new Complex(real, img)));
            return true;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage] // reason: used by external APIs only
        public override bool Dump(IQArray<Qubit>? qubits = null)
        {
            Values = new List<(int Idx, Complex Value)>();
            return base.Dump(qubits);
        }
    }
}
