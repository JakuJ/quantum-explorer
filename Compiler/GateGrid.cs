using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace Compiler
{
    /// <inheritdoc cref="System.IEquatable{GateGrid}" />
    /// <summary>A simple class representing a grid of quantum gates.</summary>
    public class GateGrid : ICloneable, IEquatable<GateGrid>
    {
        private readonly Queue<QuantumGate>[] lanes;

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        /// <param name="qubits">The number of qubits in this circuit.</param>
        public GateGrid(int qubits)
        {
            lanes = new Queue<QuantumGate>[qubits];

            for (var i = 0; i < qubits; i++)
            {
                lanes[i] = new Queue<QuantumGate>();
            }
        }

        /// <summary>Initializes a new instance of the <see cref="GateGrid"/> class.</summary>
        /// <param name="grid">A jagged 2D array of quantum gates.</param>
        public GateGrid(QuantumGate[][] grid)
        {
            int qubits = grid.GetLength(0);
            lanes = new Queue<QuantumGate>[qubits];

            for (var q = 0; q < qubits; q++)
            {
                lanes[q] = new Queue<QuantumGate>(grid[q]);
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
                    var layer = new QuantumGate[Qubits];
                    onlyIdentities = true;

                    foreach ((int index, Queue<QuantumGate> lane) in lanes.Enumerate())
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

        /// <summary>Gets the number of qubits in this grid.</summary>
        private int Qubits => lanes.Length;

        public static bool operator ==(GateGrid? left, GateGrid? right) => Equals(left, right);

        public static bool operator !=(GateGrid? left, GateGrid? right) => !(left == right);

        /// <summary>Add a gate to the grid.</summary>
        /// <param name="qubit">Zero-based index of the qubit this gate is supposed to be applied to.</param>
        /// <param name="gate">The name of the gate.</param>
        public void AddGate(int qubit, QuantumGate gate)
        {
            if (qubit < 0 || qubit >= Qubits)
            {
                throw new ArgumentException("Qubit out of range of the grid");
            }

            lanes[qubit].Enqueue(gate);
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
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach ((int qubit, var queue) in lanes.Enumerate())
            {
                builder.Append($"Q{qubit}: ");
                builder.AppendLine(string.Join(" ", queue.Select(x => x.Symbol)));
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public object Clone() => new GateGrid(lanes.Select(x => x.ToArray()).ToArray());

        /// <inheritdoc/>
        public bool Equals(GateGrid other)
        {
            if (Qubits != other.Qubits)
            {
                return false;
            }

            foreach (var (lane1, lane2) in lanes.Zip(other.lanes))
            {
                if (lane1.Count != lane2.Count)
                {
                    return false;
                }

                foreach (var (gate1, gate2) in lane1.Zip(lane2))
                {
                    if (gate1 != gate2)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
