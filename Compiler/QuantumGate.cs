using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <inheritdoc />
    /// <summary>A simplified representation of a quantum gate meant to be used in the compositor.</summary>
    public readonly struct QuantumGate : IEquatable<QuantumGate>
    {
        /// <summary>Initializes a new instance of the <see cref="QuantumGate"/> struct.</summary>
        /// <param name="grid">The parent grid.</param>
        /// <param name="ast">The AST element corresponding to the call to the underlying operation.</param>
        /// <param name="symbol">The name of the underlying operation.</param>
        /// <param name="ns">The namespace of the underlying operation.</param>
        /// <param name="height">The number of affected qubits.</param>
        public QuantumGate(GateGrid? grid, QsCallable? ast, string symbol, string ns, int height = 1)
        {
            Name = symbol;
            Namespace = ns;
            Height = height;
            ParentGrid = grid;
            AstElement = ast;
            ControlQubits = new List<int>();
        }

        /// <summary>Initializes a new instance of the <see cref="QuantumGate" /> struct.</summary>
        /// <param name="symbol">The name of the underlying operation.</param>
        public QuantumGate(string symbol) : this(null, null, symbol, "") { }

        /// <summary>Gets the full namespace of the represented operation.</summary>
        public string Namespace { get; }

        /// <summary>Gets the name of the represented operation.</summary>
        public string Name { get; }

        /// <summary>Gets the <see cref="GateGrid"/> which this gate belongs to.</summary>
        public GateGrid? ParentGrid { get; }

        /// <summary>Gets the number of qubits this gate takes as input.</summary>
        public int Height { get; }

        /// <summary>Gets a list of qubits in the parent grid that are used as control for this gate.</summary>
        public List<int> ControlQubits { get; }

        /// <summary>Gets the AST element corresponding to the call of the underlying operation.</summary>
        public QsCallable? AstElement { get; }

        public static bool operator ==(QuantumGate? left, QuantumGate? right) => Equals(left, right);

        public static bool operator !=(QuantumGate? left, QuantumGate? right) => !(left == right);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(Namespace, Name, ParentGrid, Height, ControlQubits, AstElement);

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj.GetType() == GetType() && Equals((QuantumGate)obj);
        }

        /// <inheritdoc/>
        public bool Equals(QuantumGate other) => Namespace == other.Namespace && Name == other.Name &&
                                                 ReferenceEquals(ParentGrid, other.ParentGrid) &&
                                                 Height == other.Height && ControlQubits.SequenceEqual(other.ControlQubits) &&
                                                 ReferenceEquals(AstElement, other.AstElement);
    }
}
