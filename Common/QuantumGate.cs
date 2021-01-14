using Newtonsoft.Json;

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
        /// <param name="name">The name of the underlying operation.</param>
        /// <param name="namespace">The namespace of the underlying operation.</param>
        /// <param name="argIndex">The index of the argument in the operation call.</param>
        public QuantumGate(string name, string @namespace = "__unknown__", int argIndex = 0)
        {
            Name = name;
            Namespace = @namespace;
            ArgIndex = argIndex;
        }

        /// <summary>
        /// Gets the full name of the gate.
        /// </summary>
        [JsonIgnore]
        public string FullName => $"{Namespace}.{Name}";

        /// <summary>Returns whether this object is a control gate.</summary>
        /// <returns>Whether this is a control gate.</returns>
        public bool IsControlGate() => Name == "__control__";

        /// <inheritdoc/>
        public override string ToString() => FullName;
    }
}
