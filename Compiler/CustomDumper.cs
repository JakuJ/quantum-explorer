using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using static Microsoft.Quantum.Simulation.Simulators.QuantumSimulator;

namespace Compiler
{
    /// <inheritdoc/>
    public class CustomDumper : StateDumper
    {
        /// <summary>
        /// Gets last dumped quantum states.
        /// </summary>
        public List<(int Idx, Complex Value)> Values { get; private set; } = new List<(int, Complex)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDumper"/> class.
        /// </summary>
        /// <param name="simulator">Simulator that is being dumped.</param>
        public CustomDumper(QuantumSimulator simulator) : base(simulator)
        {
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override bool Callback(uint idx, double real, double img)
        {
            Values.Add(((int)idx, new Complex(real, img)));
            return true;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override bool Dump(IQArray<Qubit> qubits = null)
        {
            Values = new List<(int, Complex)>();
            return base.Dump(qubits);
        }

    }
}
