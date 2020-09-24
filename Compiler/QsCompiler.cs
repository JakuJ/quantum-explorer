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
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Compilation = Microsoft.Quantum.QsCompiler.CompilationBuilder.CompilationUnitManager.Compilation;

namespace Compiler
{
    /// <inheritdoc cref="System.IDisposable" />
    /// <summary>An instance of <see cref="T:Compiler.ICompiler" /> using the official Q# compiler.</summary>
    public sealed class QsCompiler : ICompiler
    {
        // These are cached references that we link with the compiled source files
        // Loading them takes 10-20 seconds, that's why.
        private static ImmutableDictionary<NonNullable<string>, References.Headers>? refPaths;
        private readonly ILogger<QsCompiler> logger;
        private readonly CompilationUnitManager manager;
        private readonly Uri sourceUri = new Uri($"file:///tmp/{UniqueId.CreateUniqueId()}.qs");
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

        /// <inheritdoc/>
        public QsCompilation Compilation => CurrentCompilation.BuiltCompilation;

        /// <inheritdoc/>
        public IEnumerable<Diagnostic> Diagnostics => CurrentCompilation.Diagnostics().ToList();

        private Compilation CurrentCompilation
        {
            get
            {
                if (compilation == null)
                {
                    throw new InvalidOperationException("There is no compilation.");
                }

                return compilation;
            }
            set => compilation = value;
        }

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            FileContentManager file;

            using (new ScopedTimer("Initializing the file manager", logger))
            {
                file = CompilationUnitManager.InitializeFileManager(sourceUri, code);
            }

            using (new ScopedTimer("Updating source files", logger))
            {
                await manager.AddOrUpdateSourceFileAsync(file);
            }

            using (new ScopedTimer("Compiling", logger))
            {
                CurrentCompilation = manager.Build();
            }
        }

        private static void PreloadReferences(ILogger logger)
        {
            // TODO: Add other DLLs we want to support here
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
