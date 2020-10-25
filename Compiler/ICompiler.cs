using System;
using System.Threading.Tasks;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <summary>
    /// Represents entities that can analyze Q# source files.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>An event raised when the compiler has finished the simulation.</summary>
        event EventHandler<OutputReadyArgs> OutputReady;

        /// <summary>Gets the syntax tree of the last compilation.</summary>
        QsCompilation? Compilation { get; }

        /// <summary>Analyze provided Q# code.</summary>
        /// <param name="code">Q# code as a plain string.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Compile(string code);
    }
}
