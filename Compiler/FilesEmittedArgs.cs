using System;
using System.Collections.Generic;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <inheritdoc />
    /// <summary>
    /// Provides event data for for whenever the compiler generates C# code for quantum simulation.
    /// </summary>
    public class FilesEmittedArgs : EventArgs
    {
        /// <inheritdoc cref="EventArgs" />
        /// <summary>Initializes a new instance of the <see cref="FilesEmittedArgs"/> class.</summary>
        /// <param name="hash">Compilation hash.</param>
        /// <param name="contents">Generated file contents.</param>
        public FilesEmittedArgs(int hash, Dictionary<string, string> contents) =>
            (CompilationHash, FileContents) = (hash, contents);

        /// <summary>Gets the hash of the <see cref="QsCompilation"/> corresponding to the emitted files.</summary>
        public int CompilationHash { get; }

        /// <summary>Gets the files emitted during the Q# to C# code generation.</summary>
        public Dictionary<string, string> FileContents { get; }
    }
}
