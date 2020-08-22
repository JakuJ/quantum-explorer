using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations.QsCodeOutput;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Compilation = Microsoft.Quantum.QsCompiler.CompilationBuilder.CompilationUnitManager.Compilation;

namespace Compiler
{
    /// <inheritdoc cref="System.IDisposable" />
    /// <summary>An instance of <see cref="T:Compiler.ICompiler" /> using the official Q# compiler.</summary>
    public class QsCompiler : ICompiler, IDisposable
    {
        // These are cached references that we link with the compiled source files
        // Loading them takes 10-20 seconds, that's why.
        private static ImmutableDictionary<NonNullable<string>, References.Headers>? refPaths;
        private readonly ILogger<QsCompiler> logger;
        private readonly CompilationUnitManager manager;
        private Compilation? compilation;

        /// <summary>
        /// Initializes a new instance of the <see cref="QsCompiler"/> class.
        /// </summary>
        /// <param name="logger">A <see cref="Logger"/> instance to log the message with.</param>
        public QsCompiler(ILogger<QsCompiler> logger)
        {
            this.logger = logger;

            if (refPaths == null)
            {
                PreloadReferences(logger);
            }

            using (new ScopedTimer("Loading cached references", logger))
            {
                manager = new CompilationUnitManager();
                manager.UpdateReferencesAsync(new References(refPaths)).WaitAndUnwrapException();
            }
        }

        private Compilation CurrentCompilation
        {
            get
            {
                if (compilation == null)
                {
                    throw new InvalidOperationException("There is no compilation");
                }

                return compilation;
            }
            set => compilation = value;
        }

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            ImmutableHashSet<FileContentManager> files;

            using (new ScopedTimer("Initializing the file manager", logger))
            {
                // A dummy filename. It's constant, so we override any previous source files.
                var sourceFiles = new Dictionary<Uri, string> { { new Uri("file:///tmp/TempFile.qs"), code } };
                files = CompilationUnitManager.InitializeFileManagers(sourceFiles.ToImmutableDictionary());
            }

            using (new ScopedTimer("Updating source files", logger))
            {
                await manager.AddOrUpdateSourceFilesAsync(files);
            }

            using (new ScopedTimer("Compiling", logger))
            {
                CurrentCompilation = manager.Build();
            }
        }

        /// <inheritdoc/>
        public string GetCode()
        {
            SyntaxTreeToQsharp st = SyntaxTreeToQsharp.Default;
            QsCompilation comp = st.OnCompilation(CurrentCompilation.BuiltCompilation);
            return st.ToCode(comp.Namespaces.FirstOrDefault());
        }

        /// <inheritdoc/>
        public List<Diagnostic> GetDiagnostics() => CurrentCompilation.Diagnostics().ToList();

        /// <inheritdoc/>
        public void Dispose()
        {
            manager.Dispose();
        }

        private static void PreloadReferences(ILogger logger)
        {
            // TODO: Or just agree to a standard set of assemblies, cached for speed?
            // TODO: Dynamically add necessary assemblies based on 'open' directives in the code?
            string[] assemblies =
            {
                GetDllPath("Microsoft.Quantum.Standard.dll"),
                GetDllPath("Microsoft.Quantum.QSharp.Core.dll"),
            };

            using (new ScopedTimer("Preloading Q# assemblies", logger))
            {
                refPaths = ProjectManager.LoadReferencedAssemblies(assemblies);
            }
        }

        private static string GetDllPath(string dll)
        {
            var assembly = Assembly.GetExecutingAssembly();

            string? assemblyDir = Path.GetDirectoryName(assembly.Location);
            if (assemblyDir == null)
            {
                throw new ApplicationException("Couldn't get the directory of the executing assembly");
            }

            return Path.Combine(assemblyDir, dll);
        }
    }
}
