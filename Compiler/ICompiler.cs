using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Compiler
{
    /// <summary>
    /// Represents entities that can analyze Q# source files.
    /// </summary>
    public interface ICompiler
    {
        /// <summary>
        /// Analyze provided Q# code.
        /// </summary>
        /// <param name="code">Q# code as a plain string.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Compile(string code);

        /// <summary>
        /// Get the Q# code that corresponds to the current AST.
        /// </summary>
        /// <returns>Q# code.</returns>
        string GetCode();

        /// <summary>
        /// Return all compiler diagnostics from the latest call to <see cref="Compile"/>.
        /// </summary>
        /// <returns>A list of diagnostics.</returns>
        List<Diagnostic> GetDiagnostics();
    }
}
