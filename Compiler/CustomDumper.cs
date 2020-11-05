using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;
using System;
using System.Collections.Generic;
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
        public List<Complex> Values { get; private set; } = new List<Complex>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDumper"/> class.
        /// </summary>
        /// <param name="simulator">Simulator that is being dumped.</param>
        public CustomDumper(QuantumSimulator simulator) : base(simulator)
        {
        }

        /// <inheritdoc/>
        public override bool Callback(uint idx, double real, double img)
        {
            Values.Add(new Complex(real, img));
            return true;
        }

        /// <inheritdoc/>
        public override bool Dump(IQArray<Qubit> qubits = null)
        {
            Values = new List<Complex>();
            return base.Dump(qubits);
        }

    }
}
