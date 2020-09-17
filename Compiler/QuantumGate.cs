using System;

namespace Compiler
{
    /// <inheritdoc />
    /// <summary>A simplified representation of a quantum gate meant to be used in the compositor.</summary>
    public class QuantumGate : IEquatable<QuantumGate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuantumGate"/> class.
        /// </summary>
        /// <param name="symbol">The symbol for the gate.</param>
        public QuantumGate(string symbol) => Symbol = symbol;

        /// <summary>Gets the symbol of the grid (e.g. "H" or "X").</summary>
        public string Symbol { get; }

        public static bool operator ==(QuantumGate? left, QuantumGate? right) => Equals(left, right);

        public static bool operator !=(QuantumGate? left, QuantumGate? right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => Symbol.GetHashCode();

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

        /// <inheritdoc/>
        public bool Equals(QuantumGate? other) => Symbol == other?.Symbol;
    }
}
