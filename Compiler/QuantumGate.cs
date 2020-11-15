using System;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <inheritdoc />
    /// <summary>A simplified representation of a quantum gate meant to be used in the composer.</summary>
    public class QuantumGate : IEquatable<QuantumGate>
    {
        /// <summary>Initializes a new instance of the <see cref="QuantumGate"/> class.</summary>
        /// <param name="ast">The AST element corresponding to the call to the underlying operation.</param>
        /// <param name="symbol">The name of the underlying operation.</param>
        /// <param name="ns">The namespace of the underlying operation.</param>
        /// <param name="argIndex">The index of the argument in the operation call.</param>
        /// <param name="argArray">Whether this gate is a part of an array-like argument.</param>
        public QuantumGate(string symbol, string ns, int argIndex = 0, bool argArray = false, TypedExpression? ast = null)
        {
            Name = symbol;
            Namespace = ns;
            ArgIndex = argIndex;
            ArgArray = argArray;
            AstElement = ast;
        }

        /// <summary>Initializes a new instance of the <see cref="QuantumGate" /> class.</summary>
        /// <param name="symbol">The name of the underlying operation.</param>
        public QuantumGate(string symbol) : this(symbol, "Test") { }

        /// <summary>Gets the full namespace of the represented operation.</summary>
        public string Namespace { get; }

        /// <summary>Gets the name of the represented operation.</summary>
        public string Name { get; }

        /// <summary>Gets the index of the argument to the operation call this gate represents.</summary>
        public int ArgIndex { get; }

        /// <summary>Gets a value indicating whether this gate representa an argument that is part of an array.</summary>
        public bool ArgArray { get; }

        /// <summary>Gets the AST element corresponding to the call of the underlying operation.</summary>
        public TypedExpression? AstElement { get; }

        /// <summary>Returns whether two gates represent different arguments of the same operation call.</summary>
        /// <param name="other">The other gate.</param>
        /// <returns>Whether this and the other gate are part of the same operation call.</returns>
        public bool SameOperation(QuantumGate? other)
            => other != null
            && Namespace == other.Namespace
            && Name == other.Name
            && ArgIndex != other.ArgIndex
            && ReferenceEquals(AstElement, other.AstElement);

        /// <inheritdoc/>
        public bool Equals(QuantumGate? other)
            => other != null && ReferenceEquals(AstElement, other.AstElement);

        /// <inheritdoc/>
        public override string ToString() => Namespace + "." + Name;
    }
}
