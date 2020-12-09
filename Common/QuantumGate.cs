namespace Common
{
    /// <summary>A simplified representation of a quantum gate meant to be used in the compositor.</summary>
    public readonly struct QuantumGate
    {
        /// <summary>The name of the represented operation.</summary>
        public readonly string Name;

        /// <summary>The index of the argument to the operation call this gate represents.</summary>
        public readonly int ArgIndex;

        /// <summary>The namespace of the represented operation.</summary>
        public readonly string Namespace;

        /// <summary>Initializes a new instance of the <see cref="QuantumGate"/> struct.</summary>
        /// <param name="symbol">The name of the underlying operation.</param>
        /// <param name="ns">The namespace of the underlying operation.</param>
        /// <param name="argIndex">The index of the argument in the operation call.</param>
        public QuantumGate(string symbol, string ns = "__unknown__", int argIndex = 0)
        {
            Name = symbol;
            Namespace = ns;
            ArgIndex = argIndex;
        }

        /// <summary>Returns whether two gates represent different arguments of the same operation call.</summary>
        /// <param name="other">The other gate.</param>
        /// <returns>Whether this and the other gate are part of the same operation call.</returns>
        public bool SameOperation(QuantumGate? other)
            => other.HasValue
            && Namespace == other.Value.Namespace
            && Name == other.Value.Name
            && ArgIndex != other.Value.ArgIndex;

        /// <inheritdoc/>
        public override string ToString() => Namespace + "." + Name;
    }
}