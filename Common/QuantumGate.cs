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
        private readonly string @namespace;

        /// <summary>Initializes a new instance of the <see cref="QuantumGate"/> struct.</summary>
        /// <param name="symbol">The name of the underlying operation.</param>
        /// <param name="ns">The namespace of the underlying operation.</param>
        /// <param name="argIndex">The index of the argument in the operation call.</param>
        public QuantumGate(string symbol, string ns = "Test", int argIndex = 0)
        {
            Name = symbol;
            @namespace = ns;
            ArgIndex = argIndex;
        }

        /// <summary>Returns whether two gates represent different arguments of the same operation call.</summary>
        /// <param name="other">The other gate.</param>
        /// <returns>Whether this and the other gate are part of the same operation call.</returns>
        public bool SameOperation(QuantumGate? other)
            => other.HasValue
            && @namespace == other.Value.@namespace
            && Name == other.Value.Name
            && ArgIndex != other.Value.ArgIndex;

        /// <inheritdoc/>
        public override string ToString() => @namespace + "." + Name;
    }
}
