using System;

namespace Compiler
{
    /// <summary>A simplified representation of a quantum gate meant to be used in the compositor.</summary>
    public class QuantumGate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuantumGate"/> class.
        /// </summary>
        /// <param name="symbol">The symbol for the gate.</param>
        /// <param name="qubits">How many qubits this gate operates on. Defaults to 1.</param>
        public QuantumGate(string symbol, int qubits = 1) => (Symbol, Qubits) = (symbol, qubits);

        /// <summary>Gets the symbol of the grid (e.g. "H" or "X").</summary>
        public string Symbol { get; }

        /// <summary>Gets how many qubits this gate operates on.</summary>
        public int Qubits { get; }

        public static bool operator ==(QuantumGate? left, QuantumGate? right) => Equals(left, right);

        public static bool operator !=(QuantumGate? left, QuantumGate? right) => !Equals(left, right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Symbol, Qubits);

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType() && Equals((QuantumGate)obj);
        }

        private bool Equals(QuantumGate other) => Symbol == other.Symbol && Qubits == other.Qubits;
    }
}
