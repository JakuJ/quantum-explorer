using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.CsharpGeneration;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.BasicTransformations;

namespace Compiler
{
    /// <inheritdoc />
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class InMemoryEmitter : IRewriteStep
    {
        public static event EventHandler<FilesEmittedArgs>? FilesGenerated;

        public string Name => "InMemoryCsharpGeneration";

        public int Priority => -2;

        public IEnumerable<IRewriteStep.Diagnostic> GeneratedDiagnostics { get; } = new List<IRewriteStep.Diagnostic>();

        public IDictionary<string, string?> AssemblyConstants { get; } = new Dictionary<string, string?>();

        public bool ImplementsPreconditionVerification => false;

        public bool ImplementsTransformation => true;

        public bool ImplementsPostconditionVerification => false;

        public bool Transformation(QsCompilation compilation, out QsCompilation transformed)
        {
            var context = CodegenContext.Create(compilation, AssemblyConstants);
            ImmutableHashSet<NonNullable<string>>? sources = GetSourceFiles.Apply(compilation.Namespaces);

            var generatedFiles = new Dictionary<string, string>();

            foreach (NonNullable<string> source in sources.Where(s => !s.Value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                string? content = SimulationCode.generate(source, context);
                generatedFiles.Add(source.Value, content);
            }

            if (!compilation.EntryPoints.IsEmpty)
            {
                QsCallable? callable = context.allCallables.First(c => c.Key.Equals(compilation.EntryPoints.First())).Value;
                string? content = EntryPoint.generate(context, callable);
                NonNullable<string> entryPointName = NonNullable<string>.New(callable.SourceFile.Value + ".EntryPoint");
                generatedFiles.Add(entryPointName.Value, content);
            }

            FilesGenerated?.Invoke(this, new FilesEmittedArgs(compilation.GetHashCode(), generatedFiles));

            transformed = compilation;
            return true;
        }

        public bool PreconditionVerification(QsCompilation compilation) => throw new NotImplementedException();

        public bool PostconditionVerification(QsCompilation compilation) => throw new NotImplementedException();
    }
}
