namespace Common
{
    /// <summary>A class responsible for creating custom <see cref="QuantumGate"/> instances.</summary>
    public static class CustomGateFactory
    {
        /// <inheritdoc cref="QuantumGate"/>
        /// <summary>
        /// Creates a new instance of the <see cref="QuantumGate" /> class with the "__custom__" namespace.
        /// </summary>
        /// <param name="symbol">The identifier for the gate.</param>
        public static QuantumGate MakeCustomGate(string symbol) => new(symbol, "__custom__");
    }
}
