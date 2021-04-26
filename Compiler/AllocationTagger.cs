using System;
using System.Collections.Generic;
using AstTransformations;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <inheritdoc cref="IRewriteStep"/>
    /// <summary>
    /// A custom compilation step that adds calls to <see cref="Simulator.Custom.TagAllocation"/> after each "using" statement.
    /// </summary>
    public class AllocationTagger : IRewriteStep
    {
        private readonly List<string> userNamespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllocationTagger"/> class.
        /// </summary>
        /// <param name="userNamespaces">A list of user-defined namespaces.</param>
        public AllocationTagger(List<string> userNamespaces) => this.userNamespaces = userNamespaces;

        /// <inheritdoc/>
        public string Name => "Allocation tagging";

        /// <inheritdoc/>
        public int Priority => -1;

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
            transformed = AllocationTagging.TagAllocations(compilation, userNamespaces.ToArray());
            return true;
        }

        /// <inheritdoc/>
        public bool PreconditionVerification(QsCompilation compilation) => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool PostconditionVerification(QsCompilation compilation) => throw new NotImplementedException();
    }
}
