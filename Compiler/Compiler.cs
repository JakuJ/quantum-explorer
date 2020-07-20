using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.QsCodeOutput;
using Compilation = Microsoft.Quantum.QsCompiler.CompilationBuilder.CompilationUnitManager.Compilation;

namespace CompilerExtensions
{
    public class Compiler : ICompiler
    {
        private Compilation compilation;

        public async Task Compile(string filepath)
        {
            var sourceFiles = ProjectManager.LoadSourceFiles(new[] {filepath});
            var files = CompilationUnitManager.InitializeFileManagers(sourceFiles);

            using var manager = new CompilationUnitManager();
            await manager.UpdateReferencesAsync(new References(ImmutableDictionary<NonNullable<string>, References.Headers>.Empty));
            await manager.AddOrUpdateSourceFilesAsync(files);
            compilation = manager.Build();
        }

        public string GetCode()
        {
            SyntaxTreeToQsharp st = SyntaxTreeToQsharp.Default;
            QsCompilation comp = st.OnCompilation(compilation.BuiltCompilation);
            return st.ToCode(comp.Namespaces.FirstOrDefault());
        }

        public List<string> GetDiagnostics() => compilation.Diagnostics().Select(x => x.Message).ToList();
    }
}
