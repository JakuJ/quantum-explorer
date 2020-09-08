using System;
using System.Collections.Generic;
using Common;

namespace Compiler
{
    /// <summary>A simple class representing a grid of quantum gates.</summary>
    public class GateGrid
    {
        private readonly int qubits;
        private readonly Queue<QuantumGate>[] gates;

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        /// <param name="qubits">The number of qubits in this circuit.</param>
        public GateGrid(int qubits)
        {
            this.qubits = qubits;
            gates = new Queue<QuantumGate>[qubits];

            for (var i = 0; i < qubits; i++)
            {
                gates[i] = new Queue<QuantumGate>();
            }
        }

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="qubit">Zero-based index of the qubit this gate is supposed to be applied to.</param>
        /// <param name="gate">The name of the gate.</param>
        public void AddGate(int qubit, QuantumGate gate)
        {
            if (qubit < 0 || qubit >= qubits)
            {
                throw new ArgumentException("Qubit out of range of the grid");
            }

            gates[qubit].Enqueue(gate);
        }

        /// <summary>Gets the layers of the circuit.</summary>
        public IEnumerable<QuantumGate[]> Layers
        {
            get
            {
                bool onlyIdentities;

                do
                {
                    var layer = new QuantumGate[qubits];
                    onlyIdentities = true;

                    foreach ((int index, Queue<QuantumGate> lane) in gates.Enumerate())
                    {
                        if (lane.Count > 0)
                        {
                            layer[index] = lane.Dequeue();
                            onlyIdentities = false;
                        }
                        else
                        {
                            layer[index] = new QuantumGate("I");
                        }
                    }

                    if (!onlyIdentities)
                    {
                        yield return layer;
                    }
                }
                while (!onlyIdentities);
            }
        }
    }
}
