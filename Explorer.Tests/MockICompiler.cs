using System.Collections.Generic;
using System.Threading.Tasks;
using Compiler;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Explorer.Tests
{
    /// <inheritdoc />
    public class MockICompiler : ICompiler
    {
        /// <inheritdoc/>
        public Task Compile(string code) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public string GetCode() => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public List<Diagnostic> GetDiagnostics() => throw new System.NotImplementedException();
    }
}
