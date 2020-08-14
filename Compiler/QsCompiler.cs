using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.QsCodeOutput;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Compilation = Microsoft.Quantum.QsCompiler.CompilationBuilder.CompilationUnitManager.Compilation;

namespace Compiler
{
    /// <inheritdoc />
    /// <summary>An instance of <see cref="ICompiler"/> using the official Q# compiler.</summary>
    public class QsCompiler : ICompiler
    {
        private Compilation compilation;

        private static string GetDllPath(string dll)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir == null)
            {
                throw new ApplicationException("Couldn't get the directory of the executing assembly");
            }

            return Path.Combine(assemblyDir, dll);
        }

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            var sourceFiles = new Dictionary<Uri, string>
            {
                // Dummy filepath to a nonexistent file
                {new Uri("file:///tmp/TempFile.qs"), code}
            }.ToImmutableDictionary();

            ImmutableHashSet<FileContentManager> files = CompilationUnitManager.InitializeFileManagers(sourceFiles);

            using var manager = new CompilationUnitManager();

            // These are the assemblies that are linked with the compiled file
            // TODO: Dynamically find necessary assemblies based on 'open' directives in the code.
            string[] paths =
            {
                GetDllPath("Microsoft.Quantum.Standard.dll"),
                GetDllPath("Microsoft.Quantum.QSharp.Core.dll"),
            };

            ImmutableDictionary<NonNullable<string>, References.Headers> dict = ProjectManager.LoadReferencedAssemblies(paths);
            await manager.UpdateReferencesAsync(new References(dict));

            await manager.AddOrUpdateSourceFilesAsync(files);
            compilation = manager.Build();
        }

        /// <inheritdoc/>
        public string GetCode()
        {
            SyntaxTreeToQsharp st = SyntaxTreeToQsharp.Default;
            QsCompilation comp = st.OnCompilation(compilation.BuiltCompilation);
            return st.ToCode(comp.Namespaces.FirstOrDefault());
        }

        /// <inheritdoc/>
        public List<Diagnostic> GetDiagnostics() => compilation.Diagnostics().ToList();
    }
}
