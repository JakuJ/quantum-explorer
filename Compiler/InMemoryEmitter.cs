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
    internal class InMemoryEmitter : IRewriteStep
    {
        public static Dictionary<string, string> GeneratedFiles { get; } = new Dictionary<string, string>();

        public string Name => "InMemoryCsharpGeneration";

        public int Priority => -2;

        public IEnumerable<IRewriteStep.Diagnostic> GeneratedDiagnostics { get; } = new List<IRewriteStep.Diagnostic>();

        public IDictionary<string, string> AssemblyConstants { get; } = new Dictionary<string, string>();

        public bool ImplementsPreconditionVerification => false;

        public bool ImplementsTransformation => true;

        public bool ImplementsPostconditionVerification => false;

        public bool Transformation(QsCompilation compilation, out QsCompilation transformed)
        {
            var context = CodegenContext.Create(compilation, AssemblyConstants);
            ImmutableHashSet<NonNullable<string>>? sources = GetSourceFiles.Apply(compilation.Namespaces);

            foreach (NonNullable<string> source in sources.Where(s => !s.Value.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)))
            {
                string? content = SimulationCode.generate(source, context);
                GeneratedFiles.Add(source.Value, content);
            }

            if (!compilation.EntryPoints.IsEmpty)
            {
                QsCallable? callable = context.allCallables.First(c => c.Key.Equals(compilation.EntryPoints.First())).Value;
                string? content = EntryPoint.generate(context, callable);
                NonNullable<string> entryPointName = NonNullable<string>.New(callable.SourceFile.Value + ".EntryPoint");
                GeneratedFiles.Add(entryPointName.Value, content);
            }

            transformed = compilation;
            return true;
        }

        public bool PreconditionVerification(QsCompilation compilation) => throw new NotImplementedException();

        public bool PostconditionVerification(QsCompilation compilation) => throw new NotImplementedException();
    }
}
