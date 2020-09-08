using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        /// <param name="grid">A jagged 2D array of quantum gates.</param>
        public GateGrid(QuantumGate[][] grid)
        {
            qubits = grid.GetLength(0);
            gates = new Queue<QuantumGate>[qubits];

            for (var q = 0; q < qubits; q++)
            {
                gates[q] = new Queue<QuantumGate>();
                for (var g = 0; g < grid[q].Length; g++)
                {
                    gates[q].Enqueue(grid[q][g]);
                }
            }
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

        public static bool operator ==(GateGrid? left, GateGrid? right) => Equals(left, right);

        public static bool operator !=(GateGrid? left, GateGrid? right) => !Equals(left, right);

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

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            return obj.GetType() == typeof(GateGrid) && Equals((GateGrid)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach ((int qubit, var queue) in gates.Enumerate())
            {
                builder.Append($"Q{qubit}: ");
                builder.AppendLine(string.Join("|", queue.Select(x => x.Symbol)));
            }

            return builder.ToString();
        }

        private bool Equals(GateGrid other)
        {
            if (qubits != other.qubits || gates.Length != other.gates.Length)
            {
                return false;
            }

            return gates.Zip(other.gates).All(pair =>
            {
                var (first, second) = pair;
                foreach (var (gate1, gate2) in first.Zip(second))
                {
                    if (gate1 != gate2)
                    {
                        return false;
                    }
                }

                return true;
            });
        }
    }
}
