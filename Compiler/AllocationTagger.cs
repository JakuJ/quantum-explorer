using System;
using System.Collections.Generic;
using AstTransformations;
using Microsoft.CodeAnalysis;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    public class AllocationTagger : IRewriteStep
    {
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
        public IEnumerable<IRewriteStep.Diagnostic> GeneratedDiagnostics => new List<IRewriteStep.Diagnostic>();

        /// <inheritdoc/>
        public IDictionary<string, string?> AssemblyConstants { get; } = new Dictionary<string, string?>();

        /// <inheritdoc/>
        public bool Transformation(QsCompilation compilation, out QsCompilation transformed)
        {
            transformed = TagAllocations.TagAllocationsInCompilation(compilation);
            Console.WriteLine("Tagged allocations");
            return true;
        }

        /// <inheritdoc/>
        public bool PreconditionVerification(QsCompilation compilation) => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public bool PostconditionVerification(QsCompilation compilation) => throw new System.NotImplementedException();
    }
}
