using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Compiler
{
    /// <inheritdoc />
    /// <remarks>
    /// Inspired by <see cref="!:https://www.strathweb.com/2020/08/running-q-compiler-and-simulation-programmatically-from-a-c-application/"/>.
    /// </remarks>
    public class QsCompiler : ICompiler
    {
        private static string[]? qsharpReferences;
        private static string[]? csharpReferences;
        private static References? cachedRefs;

        private readonly ILogger<QsCompiler> logger;
        private readonly string filename = $"__{UniqueId.CreateUniqueId()}__.qs";
        private readonly List<FilesEmittedArgs> eventQueue = new List<FilesEmittedArgs>();

        private static void InitializeReferences()
        {
            // necessary references to compile our Q# program
            qsharpReferences ??= new[]
            {
                "Microsoft.Quantum.Standard",
                "Microsoft.Quantum.QSharp.Core",
                "Microsoft.Quantum.Runtime.Core",
            }.Select(x => Assembly.Load(new AssemblyName(x))).Select(a => a.Location).ToArray();

            // necessary references to compile C# simulation of the Q# compilation
            csharpReferences ??= new[]
            {
                "Microsoft.Quantum.Simulators",
                "Microsoft.Quantum.EntryPointDriver",
                "System.CommandLine",
                "System.Runtime",
                "netstandard",
                "System.Collections.Immutable",
                typeof(object).Assembly.FullName,
            }.Select(x => Assembly.Load(new AssemblyName(x!))).Select(a => a.Location).Concat(qsharpReferences).ToArray();
        }

        /// <summary>Initializes a new instance of the <see cref="QsCompiler"/> class.</summary>
        /// <param name="logger">A <see cref="Logger"/> instance to log compilation messages with.</param>
        public QsCompiler(ILogger<QsCompiler> logger)
        {
            this.logger = logger;
            InitializeReferences();
        }

        /// <inheritdoc/>
        public event EventHandler<string>? OnDiagnostics;

        /// <inheritdoc/>
        public event EventHandler<QsCompilation>? OnCompilation;

        /// <inheritdoc/>
        public event EventHandler<string>? OnOutput;

        /// <inheritdoc/>
        public QsCompilation? Compilation { get; private set; }

        /// <inheritdoc/>
        public async Task Compile(string qsharpCode, bool execute = false)
        {
            // to load our custom rewrite step, we need to point Q# compiler config at our current assembly
            var config = new CompilationLoader.Configuration
            {
                IsExecutable = execute,
                RewriteSteps = new List<(string, string?)>
                {
                    (Assembly.GetExecutingAssembly().Location, null),
                },
            };

            void Handler(object? sender, FilesEmittedArgs? args)
            {
                eventQueue.Add(args!);
            }

            eventQueue.Clear();

            InMemoryEmitter.FilesGenerated += Handler;

            // compile Q# code
            CompilationLoader? compilationLoader;

            using (new ScopedTimer("Compiling Q# code", logger))
            {
                compilationLoader = new CompilationLoader(
                    _ => new Dictionary<Uri, string> { { new Uri(Path.GetFullPath(filename)), qsharpCode } }.ToImmutableDictionary(),
                    load => cachedRefs ??= load(qsharpReferences),
                    config,
                    new EventLogger(str => OnDiagnostics?.Invoke(this, str)));
            }

            InMemoryEmitter.FilesGenerated -= Handler;

            // print any diagnostics
            var diags = compilationLoader.LoadDiagnostics;
            if (diags.Any())
            {
                var diagnostics = string.Join(Environment.NewLine, diags.Select(d => $"{d.Severity} {d.Code} {d.Message}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (diags.Any(d => d.Severity == Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity.Error))
                {
                    logger.LogWarning("There were errors in Q# compilation, aborting");
                    return;
                }
            }

            // communicate that the Q# compilation was successful
            Compilation = compilationLoader.CompilationOutput;

            if (Compilation == null || !execute)
            {
                return;
            }

            OnCompilation?.Invoke(this, Compilation);

            // find our generated files
            Dictionary<string, string>? generatedFiles = null;
            foreach (var args in eventQueue)
            {
                if (args.CompilationHash == Compilation.GetHashCode())
                {
                    generatedFiles = args.FileContents;
                    break;
                }
            }

            if (generatedFiles == null)
            {
                logger.LogError("Couldn't find generated files in the event queue");
                return;
            }

            CSharpCompilation? csharpCompilation;
            using (new ScopedTimer("Compiling C# driver code", logger))
            {
                // we captured the emitted C# syntax trees into a static variable in the rewrite step
                IEnumerable<SyntaxTree> syntaxTrees = generatedFiles.Select(x => CSharpSyntaxTree.ParseText(x.Value));

                // compile C# code
                // make sure to pass in the C# references as Roslyn's metadata references
                csharpCompilation = CSharpCompilation.Create("qsharp-assembly", syntaxTrees)
                                                     .WithReferences(csharpReferences!.Select(x => MetadataReference.CreateFromFile(x)));
            }

            // print any diagnostics
            var csharpDiagnostics = csharpCompilation
                                   .GetDiagnostics()
                                   .Where(d => d is { Severity: not Hidden, Id: not "CS1701" or "CS1702" })
                                   .ToList();
            if (csharpDiagnostics.Any())
            {
                var diagnostics = string.Join("", csharpDiagnostics.Select(d => $"\n{d.Severity} {d.Id} {d.GetMessage()}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (csharpDiagnostics.Any(d => d.Severity == Error))
                {
                    return;
                }
            }

            using var timer = new ScopedTimer("Executing simulation", logger);

            // emit C# code into an in-memory assembly
            await using var peStream = new MemoryStream();
            csharpCompilation.Emit(peStream);
            peStream.Position = 0;

            // load that assembly
            var qsharpLoadContext = new QSharpLoadContext();
            Assembly qsharpAssembly = qsharpLoadContext.LoadFromStream(peStream);

            // get the @Entrypoint() operation
            QsQualifiedName? entryPoint = compilationLoader.CompilationOutput.EntryPoints.FirstOrDefault();

            if (entryPoint == null)
            {
                logger.LogError("No entrypoint in Q# compilation.");
                qsharpLoadContext.Unload();
                return;
            }

            Type? type = qsharpAssembly.GetExportedTypes().FirstOrDefault(x => x.Name == entryPoint.Name.Value);

            if (type != null)
            {
                using var sim = new InterceptingSimulator();

                // simulate the entry point operation using reflection
                object? invocation = type.InvokeMember("Run", BindingFlags.InvokeMethod, null, type, new object?[] { sim });

                if (invocation is Task task)
                {
                    await task;
                }

                OnOutput?.Invoke(this, sim.Messages);
            }
            else
            {
                logger.LogError("Q# entrypoint not found in loaded assembly.");
            }

            qsharpLoadContext.Unload();
        }
    }
}
