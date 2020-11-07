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
        /// <param name="compilationHash">Compilation hash.</param>
        /// <param name="fileContents">Generated file contents.</param>
        public FilesEmittedArgs(int compilationHash, Dictionary<string, string> fileContents)
        {
            CompilationHash = compilationHash;
            FileContents = fileContents;
        }

        /// <summary>Gets the hash of the <see cref="QsCompilation"/> corresponding to the emitted files.</summary>
        public int CompilationHash { get; }

        /// <summary>Gets the files emitted during the Q# to C# code generation.</summary>
        public Dictionary<string, string> FileContents { get; }
    }
}
