using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <summary>
    /// Represents entities that can analyze Q# source files.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>An event invoked when the compiler has diagnostics.</summary>
        event EventHandler<string> OnDiagnostics;

        /// <summary>An event raised when the compilation object is ready.</summary>
        event EventHandler<QsCompilation> OnCompilation;

        /// <summary>An event raised when there's been some output printed by the simulation.</summary>
        event EventHandler<string> OnOutput;

        /// <summary>An event raised when there are quantum states recorded for each operation.</summary>
        event EventHandler<List<OperationState>> OnStatesRecorded;

        /// <summary>Gets the last compilation if successful, otherwise null.</summary>
        QsCompilation? Compilation { get; }

        /// <summary>Compile and run provided Q# code.</summary>
        /// <param name="code">Q# code as a plain string.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Compile(string code);
    }
}
