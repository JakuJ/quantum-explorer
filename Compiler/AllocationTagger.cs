using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    public class AllocationTagger : IRewriteStep
    {
        private List<IRewriteStep.Diagnostic> diagnostics = new List<IRewriteStep.Diagnostic>();

        /// <inheritdoc/>
        public string Name => "AllocationTagging";

        /// <inheritdoc/>
        public int Priority => -2;

        /// <inheritdoc/>
        public bool ImplementsPostconditionVerification => false;

        /// <inheritdoc/>
        public bool ImplementsPreconditionVerification => false;

        /// <inheritdoc/>
        public bool ImplementsTransformation => true;

        /// <inheritdoc/>
        public IEnumerable<IRewriteStep.Diagnostic> GeneratedDiagnostics => diagnostics;

        /// <inheritdoc/>
        public IDictionary<string, string?> AssemblyConstants { get; } = new Dictionary<string, string?>();

        /// <inheritdoc/>
        public bool Transformation(QsCompilation compilation, out QsCompilation? transformed)
        {
            transformed = compilation;
            diagnostics.Add(new IRewriteStep.Diagnostic()
            {
                Message = "Applying custom rewrite step that tags allocations",
                Severity = DiagnosticSeverity.Warning,
                Stage = IRewriteStep.Stage.Transformation,
            });
            return true;
        }

        /// <inheritdoc/>
        public bool PreconditionVerification(QsCompilation compilation) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public bool PostconditionVerification(QsCompilation compilation) => throw new System.NotImplementedException();
    }
}
