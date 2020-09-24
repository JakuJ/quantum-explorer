using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Compiler
{
    /// <summary>
    /// Represents entities that can analyze Q# source files.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// Gets all compiler diagnostics from the latest call to <see cref="Compile"/>.
        /// </summary>
        IEnumerable<Diagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets the syntax tree of the last compilation.
        /// </summary>
        QsCompilation Compilation { get; }

        /// <summary>
        /// Analyze provided Q# code.
        /// </summary>
        /// <param name="code">Q# code as a plain string.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Compile(string code);
    }
}
